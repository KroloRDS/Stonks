namespace Stonks.Common.Utils;

public static class ExtensionMethods
{
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
