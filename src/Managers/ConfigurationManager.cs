namespace Stonks.Managers;

public class ConfigurationManager : IConfigurationManager
{
    private readonly IConfiguration _config;
    private const string BATTLEROYALE_FUN_WEIGHT = "BATTLEROYALE_FUN_WEIGHT";
    private const string BATTLEROYALE_STOCK_AMOUNT_WEIGHT = "BATTLEROYALE_STOCK_AMOUNT_WEIGHT";
    private const string BATTLEROYALE_VOLATILITY_WEIGHT = "BATTLEROYALE_VOLATILITY_WEIGHT";
    private const string BATTLEROYALE_MARKET_CAP_WEIGHT = "BATTLEROYALE_MARKET_CAP_WEIGHT";
    private const string BATTLEROYALE_NEW_STOCKS_AFTER_ROUND = "BATTLEROYALE_NEW_STOCKS";

    private const string PAYPAL_BUSINESS = "PAYPAL_BUSINESS";
    private const string PAYPAL_MODE = "PAYPAL_MODE";
    private const string PAYPAL_CLIENT_ID = "PAYPAL_CLIENT_ID";
    private const string PAYPAL_CLIENT_SECRET = "PAYPAL_CLIENT_SECRET";

    public ConfigurationManager(IConfiguration config)
    {
        _config = config;
    }

    public double FunWeight()
    {
        return double.Parse(_config[BATTLEROYALE_FUN_WEIGHT]);
    }

    public double StockAmountWeight()
    {
        return double.Parse(_config[BATTLEROYALE_STOCK_AMOUNT_WEIGHT]);
    }

    public double VolatilityWeight()
    {
        return double.Parse(_config[BATTLEROYALE_VOLATILITY_WEIGHT]);
    }

    public double MarketCapWeight()
    {
        return double.Parse(_config[BATTLEROYALE_MARKET_CAP_WEIGHT]);
    }

    public int NewStocksAfterRound()
    {
        return int.Parse(_config[BATTLEROYALE_NEW_STOCKS_AFTER_ROUND]);
    }

    public Dictionary<string, string> PayPalConfig()
    {
        return new Dictionary<string, string>()
        {
            { "clientId" , _config[PAYPAL_CLIENT_ID] },
            { "clientSecret", _config[PAYPAL_CLIENT_SECRET] },
            { "mode", _config[PAYPAL_MODE] },
            { "business", _config[PAYPAL_BUSINESS] }
        };
    }
}
