using Stonks.DTOs;

namespace Stonks.ExtensionMethods;

public static class ExtensionMethods
{
    public static int AssertPositive(this int number)
    {
        if (number < 1)
            throw new ArgumentOutOfRangeException(nameof(number));

        return number;
    }

    public static int AssertPositive(this int? number)
    {
        if (number is null)
            throw new ArgumentNullException(nameof(number));

        return number.Value.AssertPositive();
    }

    public static decimal AssertPositive(this decimal number)
    {
        if (number <= decimal.Zero)
            throw new ArgumentOutOfRangeException(nameof(number));

        return number;
    }
    public static decimal AssertPositive(this decimal? number)
    {
        if (number is null)
            throw new ArgumentNullException(nameof(number));

        return number.Value.AssertPositive();
    }

    public static void AssertNotEmpty(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            throw new ArgumentNullException(nameof(str));
    }

    public static double StandardDev(this IEnumerable<decimal>? sequence)
    {
        var count = sequence is null ? 0 : sequence.Count();
        if (count < 2) return 0;

        var average = sequence!.Average();
        sequence = sequence!.Select(x => x - average);
        var sum = (double)sequence.Sum(x => x * x);
        return Math.Sqrt(sum / count);
    }

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

        var test = 1 - (Normalise(5, 5, 0) ?? 0);
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

    public static bool IsAlphaNum(this char c)
    {
        return char.IsLetterOrDigit(c);
    }
}
