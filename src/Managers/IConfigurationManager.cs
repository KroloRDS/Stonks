namespace Stonks.Managers;

public interface IConfigurationManager
{
    double FunWeight();
    double StockAmountWeight();
    double VolatilityWeight();
    double MarketCapWeight();
    int NewStocksAfterRound();
    Dictionary<string, string> PayPalConfig();
}
