namespace Stonks.Util;

public interface IStonksConfiguration
{
	double FunWeight();
	double StockAmountWeight();
	double VolatilityWeight();
	double MarketCapWeight();
	int NewStocksAfterRound();
}

public class StonksConfiguration : IStonksConfiguration
{
	private readonly IConfiguration _config;
	private const string BATTLEROYALE_FUN_WEIGHT = "BATTLEROYALE_FUN_WEIGHT";
	private const string BATTLEROYALE_STOCK_AMOUNT_WEIGHT = "BATTLEROYALE_STOCK_AMOUNT_WEIGHT";
	private const string BATTLEROYALE_VOLATILITY_WEIGHT = "BATTLEROYALE_VOLATILITY_WEIGHT";
	private const string BATTLEROYALE_MARKET_CAP_WEIGHT = "BATTLEROYALE_MARKET_CAP_WEIGHT";
	private const string BATTLEROYALE_NEW_STOCKS_AFTER_ROUND = "BATTLEROYALE_NEW_STOCKS";

	public StonksConfiguration(IConfiguration config)
	{
		_config = config;
	}

	public double FunWeight()
	{
		return TryParse(_config[BATTLEROYALE_FUN_WEIGHT]);
	}

	public double StockAmountWeight()
	{
		return TryParse(_config[BATTLEROYALE_STOCK_AMOUNT_WEIGHT]);
	}

	public double VolatilityWeight()
	{
		return TryParse(_config[BATTLEROYALE_VOLATILITY_WEIGHT]);
	}

	public double MarketCapWeight()
	{
		return TryParse(_config[BATTLEROYALE_MARKET_CAP_WEIGHT]);
	}

	public double TryParse()
	{
		return double.Parse(_config[BATTLEROYALE_MARKET_CAP_WEIGHT]
			?? string.Empty);
	}

	private static double TryParse(string? s)
	{
		return double.TryParse(s, out var result) ? result : 1;
	}

	public int NewStocksAfterRound()
	{
		var s = _config[BATTLEROYALE_NEW_STOCKS_AFTER_ROUND];
		return int.Parse(s ?? "1000");
	}
}
