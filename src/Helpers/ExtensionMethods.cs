namespace Stonks.Helpers;
public static class ExtensionMethods
{
	public static int ToPositive(this int? amount)
	{
		if (amount is null)
			throw new ArgumentNullException(nameof(amount));

		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		return amount.Value;
	}

	public static decimal ToPositive(this decimal? price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));

		if (price <= decimal.Zero)
			throw new ArgumentOutOfRangeException(nameof(price));

		return price.Value;
	}
}
