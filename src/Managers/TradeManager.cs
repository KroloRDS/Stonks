using Microsoft.AspNetCore.Identity;
using Stonks.Data;
using Stonks.DTOs;
using Stonks.Helpers;
using Stonks.Models;

namespace Stonks.Managers;

public class TradeManager : ITradeManager
{
	private readonly IStockManager _stockManager;
	private readonly AppDbContext _ctx;

	public TradeManager(IStockManager stockManager, AppDbContext ctx)
	{
		_stockManager = stockManager;
		_ctx = ctx;
	}

	public void PlaceOffer(PlaceOfferCommand? command)
	{
		(var type, var amount, var stockId, var price) = ValidateCommand(command);

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

		var newOffer = new TradeOffer
		{
			Amount = amount,
			StockId = stockId,
			WriterId = command.WriterId.ToString(),
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
		var offer = _ctx.GetById<TradeOffer>(offerId);
		BuyStock(offer, userId, offer.Amount);
		RemoveOffer(offerId);
	}

	public void AcceptOffer(Guid? userId, Guid? offerId, int? amount)
	{
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

		if (command.StockId is null)
			throw new ArgumentNullException(nameof(command.StockId));
		var stockId = command.StockId.Value;

		var amount = ValidationHelper.PositiveAmount(command.Amount);
		_ = _ctx.GetUser(command.WriterId);

		var price = type == OfferType.Buy ?
			ValidationHelper.PositivePrice(command.BuyPrice) :
			ValidationHelper.PositivePrice(command.SellPrice);

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
			.OrderBy(x => x.BuyPrice)
			.ToList();
	}

	private List<TradeOffer> FindSellOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
			x.Type != OfferType.Buy &&
			x.Stock.Id == stockId &&
			x.SellPrice <= price)
			.OrderByDescending(x => x.SellPrice)
			.ToList();
	}
}
