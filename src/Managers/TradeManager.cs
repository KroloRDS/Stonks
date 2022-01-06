using Stonks.Data;
using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers;

public class TradeManager : ITradeManager
{
	private readonly IStockOwnershipManager _stockManager;
	private readonly IHistoricalPriceManager _historicalPriceManager;
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public TradeManager(AppDbContext ctx, IStockOwnershipManager stockManager,
		IHistoricalPriceManager historicalPrice)
	{
		_ctx = ctx;
		_stockManager = stockManager;
		_historicalPriceManager = historicalPrice;
	}

	public void PlaceOffer(PlaceOfferCommand? command)
	{
		(var type, var amount, var stockId, var price, var writerId) = ValidateCommand(command);

		// Try to match with existin offers first
		var offers = type == OfferType.Buy ?
			FindSellOffers(stockId, price) :
			FindBuyOffers(stockId, price);

		foreach (var offer in offers)
		{
			if (offer.Amount < amount)
			{
				AcceptOffer(command?.WriterId, offer.Id);
				amount -= offer.Amount;
			}
			else
			{
				AcceptOffer(command?.WriterId, offer.Id, amount);
				return;
			}
		}

		// If there are still stocks to sell / buy
		var newOffer = new TradeOffer
		{
			Amount = amount,
			StockId = stockId,
			WriterId = writerId,
			Type = type
		};

		if (type == OfferType.Buy)
			newOffer.BuyPrice = price;
		else
			newOffer.SellPrice = price;

		_ctx.Add(newOffer);
		_ctx.SaveChanges();
	}

	public void AcceptOffer(Guid? userId, Guid? offerId)
	{
		var validatedUserId = _ctx.EnsureUserExist(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		BuyStock(offer, validatedUserId, offer.Amount);
		RemoveOffer(offerId);
	}

	public void AcceptOffer(Guid? userId, Guid? offerId, int? amount)
	{
		var validatedUserId = _ctx.EnsureUserExist(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		amount = amount.AssertPositive();

		if (offer.Amount <= amount)
		{
			AcceptOffer(userId, offerId);
			return;
		}

		BuyStock(offer, validatedUserId, amount);
		offer.Amount -= amount.Value;
		_ctx.SaveChanges();
	}

	public void RemoveOffer(Guid? offerId)
	{
		if (offerId is null)
			throw new ArgumentNullException(nameof(offerId));

		//TODO: User auth
		var offer = _ctx.GetById<TradeOffer>(offerId);
		_ctx.TradeOffer.Remove(offer);
		_ctx.SaveChanges();
	}

	public void RemoveAllOffersForStock(Guid? stockId)
	{
		_ctx.EnsureExist<Stock>(stockId);
		_ctx.RemoveRange(_ctx.TradeOffer.Where(x => x.StockId == stockId));
		_ctx.SaveChanges();
	}

	private (OfferType, int, Guid, decimal, string) ValidateCommand(PlaceOfferCommand? command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.Type is null)
			throw new ArgumentNullException(nameof(command.Type));
		var type = command.Type.Value;
		
		if (type == OfferType.PublicOfferring)
			throw new ArgumentException("'Public offering' offer can only be placed by the broker", nameof(type));

		var stock = _ctx.GetById<Stock>(command.StockId);
		if (stock.Bankrupt)
			throw new InvalidOperationException("Cannot buy bankrupt stock");

		var amount = command.Amount.AssertPositive();
		var price = command.Price.AssertPositive();
		var writerId = _ctx.EnsureUserExist(command.WriterId);

		if (type == OfferType.Sell)
			ValidateOwnedAmount(writerId, stock.Id, amount);

		return (type, amount, stock.Id, price, writerId);
	}

	private void ValidateOwnedAmount(string writerId, Guid stockId, int amount)
	{
		var ownedAmount = _ctx.GetStockOwnership(writerId, stockId)?.Amount;
		if (ownedAmount is null || ownedAmount < amount)
			throw new ArgumentException("Not enough owned stock", nameof(amount));
	}

	private void BuyStock(TradeOffer offer, string userId, int? amount)
	{
		Guid? buyerId;
		Guid? sellerId = null;
		var buyFromUser = true;

		if (offer.Type == OfferType.Buy)
		{
			buyerId = Guid.Parse(offer.WriterId!);
			sellerId = Guid.Parse(userId);
		}
		else
		{
			buyerId = Guid.Parse(userId);
			if (offer.Type == OfferType.PublicOfferring)
			{
				buyFromUser = false;
			}
			else
			{
				sellerId = Guid.Parse(offer.WriterId!);
			}
		}

		_stockManager.BuyStock(new BuyStockCommand
		{
			Amount = amount,
			BuyerId = buyerId,
			SellerId = sellerId,
			BuyFromUser = buyFromUser,
			StockId = offer.StockId
		});
	}

	private List<TradeOffer> FindBuyOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer. Where(x =>
			x.Type == OfferType.Buy &&
			x.StockId == stockId &&
			x.BuyPrice >= price)
			.OrderByDescending(x => x.BuyPrice)
			.ToList();
	}

	private List<TradeOffer> FindSellOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
			x.Type != OfferType.Buy &&
			x.StockId == stockId &&
			x.SellPrice <= price)
			.OrderBy(x => x.SellPrice)
			.ToList();
	}

	public void AddPublicOffers(int amount)
	{
		var stockIds = _ctx.Stock
			.Where(x => !x.Bankrupt).Select(x => x.Id);

		foreach (var id in stockIds)
		{
			AddPublicOffers(id, amount);
		}
		_ctx.SaveChanges();
	}

	private void AddPublicOffers(Guid stockId, int amount)
	{
		var offer = _ctx.TradeOffer.Where(x =>
			x.Type == OfferType.PublicOfferring &&
			x.StockId == stockId)
			.FirstOrDefault();

		if (offer is not null && offer.Amount < amount)
		{
			offer.Amount = amount;
			_ctx.SaveChanges();
			return;
		}

		var avgPrice = _historicalPriceManager.GetCurrentPrice(stockId).AveragePrice;
		_ctx.Add(new TradeOffer
		{
			Amount = amount,
			SellPrice = avgPrice,
			StockId = stockId,
			Type = OfferType.PublicOfferring
		});
	}
}
