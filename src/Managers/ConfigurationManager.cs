namespace Stonks.Managers;

public class ConfigurationManager : IConfigurationManager
{
	private readonly IConfiguration _config;
	private const string FUN_WEIGHT = "BATTLEROYALE_FUN_WEIGHT";
	private const string STOCK_AMOUNT_WEIGHT = "BATTLEROYALE_STOCK_AMOUNT_WEIGHT";
	private const string VOLATILITY_WEIGHT = "BATTLEROYALE_VOLATILITY_WEIGHT";
	private const string MARKET_CAP_WEIGHT = "BATTLEROYALE_MARKET_CAP_WEIGHT";
	private const string NEW_STOCKS_AFTER_ROUND = "BATTLEROYALE_NEW_STOCKS";

	public ConfigurationManager(IConfiguration config)
	{
		_config = config;
	}

	public double FunWeight()
	{
		return double.Parse(_config[FUN_WEIGHT]);
	}

	public double StockAmountWeight()
	{
		return double.Parse(_config[STOCK_AMOUNT_WEIGHT]);
	}

	public double VolatilityWeight()
	{
		return double.Parse(_config[VOLATILITY_WEIGHT]);
	}

	double IConfigurationManager.MarketCapWeight()
	{
		return double.Parse(_config[MARKET_CAP_WEIGHT]);
	}

	public int NewStocksAfterRound()
	{
		return int.Parse(_config[NEW_STOCKS_AFTER_ROUND]);
	}
}
