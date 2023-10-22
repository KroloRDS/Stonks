namespace Stonks.Common.Utils;

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
