namespace Stonks.Helpers;
public class ValidationHelper
{
	public static int PositiveAmount(int? amount)
	{
		if (amount is null)
			throw new ArgumentNullException(nameof(amount));

		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		return amount.Value;
	}

	public static decimal PositivePrice(decimal? price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));

		if (price <= 0M)
			throw new ArgumentOutOfRangeException(nameof(price));

		return price.Value;
	}
}
