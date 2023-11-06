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

		var weakest = Normalise(indicators).OrderBy(GetScore).First();
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
		return await shares * price.Price;
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

	private static IEnumerable<StockIndicatorNormalised> Normalise(
		IEnumerable<StockIndicator>? indicators)
	{
		if (indicators is null || !indicators.Any())
			return Enumerable.Empty<StockIndicatorNormalised>();

		var minMarketCap = indicators.Min(x => x.MarketCap);
		var marketCapMaxDiff = indicators.Max(x => x.MarketCap) - minMarketCap;

		var minVolatility = indicators.Min(x => x.Volatility);
		var volatilityMaxDiff = indicators.Max(x => x.Volatility) - minVolatility;

		var minStocksAvailable = indicators.Min(x => x.StocksAmount);
		var stocksAvailableMaxDiff = indicators.Max(x => x.StocksAmount) - minStocksAvailable;

		return indicators.Select(x => new StockIndicatorNormalised
		{
			StockId = x.StockId,
			Fun = x.Fun,
			MarketCap = Normalise((double)x.MarketCap, (double)minMarketCap, (double)marketCapMaxDiff) ?? 1,
			StocksAmount = 1 - (Normalise(x.StocksAmount, minStocksAvailable, stocksAvailableMaxDiff) ?? 0),
			Volatility = 1 - (Normalise(x.Volatility, minVolatility, volatilityMaxDiff) ?? 0)
		});
	}

	private static double? Normalise(double x, double min, double maxDiff)
	{
		if (maxDiff == 0) return null;
		return (x - min) / maxDiff;
	}

	private class StockIndicator
	{
		public Guid StockId { get; set; }
		public decimal MarketCap { get; set; }
		public int StocksAmount { get; set; }
		public double Volatility { get; set; }
		public double Fun { get; set; }
	}

	private class StockIndicatorNormalised
	{
		public Guid StockId { get; set; }
		public double MarketCap { get; set; }
		public double StocksAmount { get; set; }
		public double Volatility { get; set; }
		public double Fun { get; set; }
	}
}
