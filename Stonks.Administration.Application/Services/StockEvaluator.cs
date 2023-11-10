using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils.ExtensionMethods;
using Stonks.Common.Utils.Models.Configuration;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Administration.Application.Services;

public interface IStockEvaluator
{
	Task<Guid> FindWeakest(CancellationToken cancellationToken = default);
}

public class StockEvaluator : IStockEvaluator
{
	private readonly IStockRepository _stock;
	private readonly IShareRepository _share;
	private readonly IPriceRepository _price;
	private readonly IOfferRepository _tradeOfferRepository;
	private readonly BattleRoyaleConfiguration _config;

	public StockEvaluator(IStockRepository stock,
		IShareRepository share,
		IPriceRepository price,
		BattleRoyaleConfiguration config,
		IOfferRepository tradeOfferRepository)
	{
		_stock = stock;
		_share = share;
		_price = price;
		_config = config;
		_tradeOfferRepository = tradeOfferRepository;
	}

	public async Task<Guid> FindWeakest(
		CancellationToken cancellationToken = default)
	{
		var stocks = await _stock.GetActive(cancellationToken);
		if (!stocks.Any()) throw new NoStocksToBankruptException();

		var indicators = await Task.WhenAll(stocks
			.Select(x => GetStockIndicator(x, cancellationToken)));

		var weakest = indicators.Normalise().OrderBy(GetScore).First();
		return weakest.StockId;
	}

	private async Task<StockIndicator> GetStockIndicator(Stock stock,
		CancellationToken cancellationToken = default)
	{
		var publicStocks = _tradeOfferRepository.PublicallyOfferdAmount(
			stock.Id, cancellationToken);
		var marketCap = GetMarketCap(stock, cancellationToken);
		var volatility = GetVolatility(stock.Id, cancellationToken);

		return new StockIndicator
		{
			StockId = stock.Id,
			MarketCap = await marketCap,
			StocksAmount = await publicStocks,
			Volatility = await volatility,
			Fun = new Random().NextDouble()
		};
	}

	private async Task<decimal> GetMarketCap(Stock stock,
		CancellationToken cancellationToken = default)
	{
		var shares = _share.TotalAmountOfShares(stock.Id, cancellationToken);
		var price = await _price.Current(stock.Id);
		return await shares * price?.Price ?? decimal.Zero;
	}

	private async Task<double> GetVolatility(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var lastBankruptDate = await _stock.LastBankruptDate(cancellationToken);
		var prices = _price.Prices(stockId, lastBankruptDate);
		return prices.Select(x => x.Price).StandardDev();
	}

	private double GetScore(StockIndicatorNormalised indicator)
	{
		return _config.MarketCapWeight * indicator.MarketCap +
			_config.StockAmountWeight * indicator.StocksAmount +
			_config.VolatilityWeight * indicator.Volatility +
			_config.FunWeight * indicator.Fun;
	}
}
