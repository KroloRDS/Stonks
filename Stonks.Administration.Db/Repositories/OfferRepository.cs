using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using Stonks.Common.Db.EntityFrameworkModels;
using Stonks.Common.Utils.Models.Constants;
using CommonRepositories = Stonks.Common.Db.Repositories;

namespace Stonks.Administration.Db.Repositories;

public class OfferRepository : IOfferRepository
{
	private readonly AppDbContext _ctx;
	private readonly IPriceRepository _price;
	private readonly CommonRepositories.IOfferRepository _offer;

	public OfferRepository(AppDbContext ctx,
		IPriceRepository price,
		CommonRepositories.IOfferRepository offer)
	{
		_ctx = ctx;
		_price = price;
		_offer = offer;
	}

	public async Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default) =>
		await _offer.PublicallyOfferdAmount(stockId, cancellationToken);

	public async Task AddNewPublicOffers(int amount,
		CancellationToken cancellationToken = default)
	{
		var stocks = _ctx.Stock.Where(stock => stock.BankruptDate == null &&
			!_ctx.TradeOffer.Any(offer => offer.StockId == stock.Id));

		foreach (var stock in stocks)
		{
			var price = await _price.Current(stock.Id);
			await _ctx.AddAsync(new TradeOffer
			{
				Amount = amount,
				Price = price?.Price ?? Constants.STOCK_DEFAULT_PRICE,
				StockId = stock.Id,
				Type = OfferType.PublicOfferring
			}, cancellationToken);
		}
	}

	public void SetExistingPublicOffersAmount(int amount)
	{
		var offers = _ctx.TradeOffer.Where(x =>
			x.Type == OfferType.PublicOfferring &&
			x.Amount < amount &&
			x.Stock.BankruptDate == null);

		foreach (var offer in offers)
			offer.Amount = amount;
	}

	public void RemoveOffers(Guid stockId)
	{
		_ctx.TradeOffer.RemoveRange(
			_ctx.TradeOffer.Where(x => x.StockId == stockId));
	}
}
