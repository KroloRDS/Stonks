namespace Stonks.Util;

public interface IStonksConfiguration
{
    double FunWeight();
    double StockAmountWeight();
    double VolatilityWeight();
    double MarketCapWeight();
    int NewStocksAfterRound();
}
