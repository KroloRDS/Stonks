namespace Stonks.Administration.Application.Services;

public class StockIndicator
{
	public Guid StockId { get; set; }
	public decimal MarketCap { get; set; }
	public int StocksAmount { get; set; }
	public double Volatility { get; set; }
	public double Fun { get; set; }
}

public class StockIndicatorNormalised
{
	public Guid StockId { get; set; }
	public double MarketCap { get; set; }
	public double StocksAmount { get; set; }
	public double Volatility { get; set; }
	public double Fun { get; set; }
}

public static class StockIdicatorExtensionMethods
{
	public static IEnumerable<StockIndicatorNormalised> Normalise(
		this IEnumerable<StockIndicator>? indicators)
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
}
