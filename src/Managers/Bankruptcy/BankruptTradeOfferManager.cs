using Stonks.Data;
using Stonks.Models;
using Stonks.Managers.Common;

namespace Stonks.Managers.Bankruptcy;

public class BankruptTradeOfferManager : IBankruptTradeOfferManager
{
	private readonly AppDbContext _ctx;
	private readonly IGetPriceManager _priceManager;

	public BankruptTradeOfferManager(AppDbContext ctx,
		IGetPriceManager historicalPriceManager)
	{
		_ctx = ctx;
		_priceManager = historicalPriceManager;
	}

	public void AddPublicOffers(int amount)
	{
		var stockIds = _ctx.Stock
			.Where(x => !x.Bankrupt).Select(x => x.Id);

		foreach (var id in stockIds)
		{
			AddPublicOffer(id, amount);
		}
	}

	private void AddPublicOffer(Guid stockId, int amount)
	{
		var offer = _ctx.TradeOffer.Where(x =>
			x.Type == OfferType.PublicOfferring &&
			x.StockId == stockId)
			.FirstOrDefault();

		if (offer is not null)
		{
			if (offer.Amount < amount) offer.Amount = amount;
			return;
		}

		var avgPrice = _priceManager.GetCurrentPrice(stockId).Amount;
		_ctx.Add(new TradeOffer
		{
			Amount = amount,
			SellPrice = avgPrice,
			StockId = stockId,
			Type = OfferType.PublicOfferring
		});
	}

	public void RemoveAllOffersForStock(Guid? stockId)
	{
		_ctx.EnsureExist<Stock>(stockId);
		_ctx.RemoveRange(_ctx.TradeOffer.Where(x => x.StockId == stockId));
	}
}
