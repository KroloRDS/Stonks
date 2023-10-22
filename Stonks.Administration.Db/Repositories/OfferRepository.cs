using Microsoft.EntityFrameworkCore;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using Stonks.Common.Db.EntityFrameworkModels;
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

	public async Task AddPublicOffers(int amount,
		CancellationToken cancellationToken = default)
	{
		var stocks = _ctx.Stock.Where(x => x.BankruptDate == null);
		foreach (var stock in stocks)
		{
			await AddPublicOffer(stock, amount, cancellationToken);
		}
	}

	public void RemoveOffers(Guid stockId)
	{
		_ctx.TradeOffer.RemoveRange(
			_ctx.TradeOffer.Where(x => x.StockId == stockId));
	}

	private async Task AddPublicOffer(Stock stock, int amount,
		CancellationToken cancellationToken)
	{
		var offer = await _ctx.TradeOffer.FirstOrDefaultAsync(x =>
			x.Type == OfferType.PublicOfferring &&
			x.StockId == stock.Id,
			cancellationToken);

		if (offer == default)
			await CreateNewPublicOffer(stock, amount, cancellationToken);
		else if (offer.Amount < amount)
			offer.Amount = amount;
	}

	private async Task CreateNewPublicOffer(Stock stock, int amount,
		CancellationToken cancellationToken)
	{
		var price = await _price.Current(stock.Id);
		await _ctx.AddAsync(new TradeOffer
		{
			Amount = amount,
			Price = price.Price,
			StockId = stock.Id,
			Type = OfferType.PublicOfferring
		}, cancellationToken);
	}
}
