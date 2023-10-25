namespace Stonks.Common.Utils.Configuration;

public record BattleRoyaleConfiguration(
    double FunWeight,
    double MarketCapWeight,
    short NewStocksAfterRound,
    double StockAmountWeight,
    double VolatilityWeight);
