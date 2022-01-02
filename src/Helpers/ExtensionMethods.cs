namespace Stonks.Helpers;
public static class ExtensionMethods
{
	public static int AssertPositive(this int? amount)
	{
		if (amount is null)
			throw new ArgumentNullException(nameof(amount));

		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		return amount.Value;
	}

	public static decimal AssertPositive(this decimal? price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));

		if (price <= decimal.Zero)
			throw new ArgumentOutOfRangeException(nameof(price));

		return price.Value;
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
}
