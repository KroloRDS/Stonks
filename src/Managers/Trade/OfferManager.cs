using Stonks.Data;
using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers.Trade;

public class OfferManager : IOfferManager
{
	private readonly IUserBalanceManager _userManager;
	private readonly ITransferSharesManager _shareManager;
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public OfferManager(AppDbContext ctx, IUserBalanceManager userManager,
		ITransferSharesManager buyStockManager)
	{
		_ctx = ctx;
		_userManager = userManager;
		_shareManager = buyStockManager;
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
				AcceptValidatedOffer(writerId, offer);
				amount -= offer.Amount;
			}
			else
			{
				AcceptValidatedOffer(writerId, offer, amount);
				_ctx.SaveChanges();
				return;
			}
		}

		// If there are still stocks to sell / buy
		TakeMoneyUpFront(type, writerId, price * amount);
		var newOffer = new TradeOffer
		{
			Amount = amount,
			StockId = stockId,
			WriterId = writerId.ToString(),
			Type = type
		};

		if (type == OfferType.Buy)
			newOffer.BuyPrice = price;
		else
			newOffer.SellPrice = price;

		_ctx.Add(newOffer);
		_ctx.SaveChanges();
	}

	private (OfferType, int, Guid, decimal, Guid) ValidateCommand(PlaceOfferCommand? command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.Type is null)
			throw new ArgumentNullException(nameof(command.Type));
		var type = command.Type.Value;

		if (type == OfferType.PublicOfferring)
			throw new PublicOfferingException();

		var stock = _ctx.GetById<Stock>(command.StockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		var amount = command.Amount.AssertPositive();
		var price = command.Price.AssertPositive();
		var writerId = _ctx.EnsureUserExist(command.WriterId);

		if (type == OfferType.Sell)
			ValidateOwnedAmount(writerId, stock.Id, amount);

		return (type, amount, stock.Id, price, writerId);
	}

	private void ValidateOwnedAmount(Guid writerId, Guid stockId, int amount)
	{
		var ownedAmount = _ctx.GetShares(writerId, stockId)?.Amount;
		if (ownedAmount is null || ownedAmount < amount)
			throw new NoStocksOnSellerException();
	}

	private List<TradeOffer> FindBuyOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
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

	public void AcceptOffer(Guid? userId, Guid? offerId)
	{
		var validatedUserId = _ctx.EnsureUserExist(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		AcceptValidatedOffer(validatedUserId, offer);
		_ctx.SaveChanges();
	}

	public void AcceptOffer(Guid? userId, Guid? offerId, int? amount)
	{
		var validatedUserId = _ctx.EnsureUserExist(userId);
		var offer = _ctx.GetById<TradeOffer>(offerId);
		AcceptValidatedOffer(validatedUserId, offer, amount.AssertPositive());
		_ctx.SaveChanges();
	}

	private void AcceptValidatedOffer(Guid userId, TradeOffer offer)
	{
		BuyShares(userId, offer, offer.Amount);
		SettleMoney(userId, offer, offer.Amount);
		_ctx.TradeOffer.Remove(offer);
	}

	private void AcceptValidatedOffer(Guid userId, TradeOffer offer, int amount)
	{
		if (offer.Amount <= amount)
		{
			AcceptValidatedOffer(userId, offer);
			return;
		}

		BuyShares(userId, offer, amount);
		SettleMoney(userId, offer, amount);
		offer.Amount -= amount;
	}

	private void SettleMoney(Guid clientId, TradeOffer offer, int amount)
	{
		var offerValue = offer.Type == OfferType.Buy ?
			offer.BuyPrice * amount :
			offer.SellPrice * amount;

		switch (offer.Type)
		{
			case OfferType.Sell:
				var writerId = _ctx.EnsureUserExist(offer.WriterId);
				_userManager.TransferMoney(clientId, writerId, offerValue);
				break;
			case OfferType.Buy:
				_userManager.GiveMoney(clientId, offerValue);
				break;
			case OfferType.PublicOfferring:
				_userManager.TakeMoney(clientId, offerValue);
				break;
		}
	}

	private void BuyShares(Guid userId, TradeOffer offer, int? amount)
	{
		Guid? buyerId;
		Guid? sellerId = null;
		var buyFromUser = true;

		if (offer.Type == OfferType.Buy)
		{
			buyerId = Guid.Parse(offer.WriterId!);
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
				sellerId = Guid.Parse(offer.WriterId!);
			}
		}

		_shareManager.TransferShares(new TransferSharesCommand
		{
			Amount = amount,
			BuyerId = buyerId,
			SellerId = sellerId,
			BuyFromUser = buyFromUser,
			StockId = offer.StockId
		});
	}

	public void CancelOffer(Guid? offerId)
	{
		if (offerId is null)
			throw new ArgumentNullException(nameof(offerId));

		//TODO: User auth
		var offer = _ctx.GetById<TradeOffer>(offerId);

		if (offer.Type is OfferType.PublicOfferring)
			throw new PublicOfferingException();

		Refund(offer);
		_ctx.TradeOffer.Remove(offer);

		_ctx.SaveChanges();
	}

	private void Refund(TradeOffer offer)
	{
		if (offer.Type != OfferType.Buy) return;
		_userManager.GiveMoney(_ctx.EnsureUserExist(offer.WriterId),
			offer.BuyPrice * offer.Amount);
	}

	private void TakeMoneyUpFront(OfferType type,
		Guid userId, decimal offerValue)
	{
		if (type != OfferType.Buy) return;
		_userManager.TakeMoney(userId, offerValue);
	}
}
