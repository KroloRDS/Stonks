using Stonks.Data;
using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers;

public class TradeManager : ITradeManager
{
	private readonly IStockManager _stockManager;
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public TradeManager(IStockManager stockManager, AppDbContext ctx)
	{
		_stockManager = stockManager;
		_ctx = ctx;
	}

	public void PlaceOffer(PlaceOfferCommand? command)
	{
		(var type, var amount, var stockId, var price) = ValidateCommand(command);
		var writerId = command.WriterId.ToString();

		var ownedAmount = _ctx.GetStockOwnership(writerId, stockId)?.Amount;
		if (type == OfferType.Sell && (ownedAmount is null || ownedAmount < amount))
			throw new ArgumentException("Not enough owned stock", nameof(amount));

		// Try to match with existin offers first
		var offers = type == OfferType.Buy ?
			FindSellOffers(stockId, price) :
			FindBuyOffers(stockId, price);

		foreach (var offer in offers)
		{
			if (offer.Amount < amount)
			{
				AcceptOffer(command.WriterId, offer.Id);
				amount -= offer.Amount;
			}
			else
			{
				AcceptOffer(command.WriterId, offer.Id, amount);
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
		_ = _ctx.GetUser(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		BuyStock(offer, userId, offer.Amount);
		RemoveOffer(offerId);
	}

	public void AcceptOffer(Guid? userId, Guid? offerId, int? amount)
	{
		_ = _ctx.GetUser(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		amount = ValidationHelper.PositiveAmount(amount);

		if (offer.Amount <= amount)
		{
			AcceptOffer(userId, offerId);
			return;
		}

		BuyStock(offer, userId, amount);
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

	private (OfferType, int, Guid, decimal) ValidateCommand(PlaceOfferCommand command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.Type is null)
			throw new ArgumentNullException(nameof(command.Type));
		var type = command.Type.Value;
		
		if (type == OfferType.PublicOfferring)
			throw new ArgumentException("'Public offering' offer can only be placed by the broker", nameof(type));

		if (command.StockId is null)
			throw new ArgumentNullException(nameof(command.StockId));
		var stockId = command.StockId.Value;
		_ = _ctx.GetById<Stock>(stockId);

		var amount = ValidationHelper.PositiveAmount(command.Amount);
		_ = _ctx.GetUser(command.WriterId);

		var price = ValidationHelper.PositivePrice(command.Price);

		return (type, amount, stockId, price);
	}

	private void BuyStock(TradeOffer offer, Guid? userId, int? amount)
	{
		Guid? buyerId;
		Guid? sellerId = null;
		var buyFromUser = true;

		if (offer.Type == OfferType.Buy)
		{
			buyerId = Guid.Parse(offer.Writer.Id);
			sellerId = userId;
		}
		else
		{
			buyerId = userId;
			if (offer.Type == OfferType.PublicOfferring)
			{
				buyFromUser = false;
			}
			else
			{
				sellerId = Guid.Parse(offer.Writer.Id);
			}
		}

		_stockManager.BuyStock(new BuyStockCommand
		{
			Amount = amount,
			BuyerId = buyerId,
			SellerId = sellerId,
			BuyFromUser = buyFromUser,
			StockId = offer.Stock.Id
		});
	}

	private List<TradeOffer> FindBuyOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer. Where(x =>
			x.Type == OfferType.Buy &&
			x.Stock.Id == stockId &&
			x.BuyPrice >= price)
			.OrderByDescending(x => x.BuyPrice)
			.ToList();
	}

	private List<TradeOffer> FindSellOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
			x.Type != OfferType.Buy &&
			x.Stock.Id == stockId &&
			x.SellPrice <= price)
			.OrderBy(x => x.SellPrice)
			.ToList();
	}
}
