namespace Stonks.Helpers;

public class Error
{
	public enum Code
	{
		OK,
		NullParameter,
		GenericError,
		CantFindUser,
		CantFindStock,
		AmountNotPositive,
		NotEnoughPublicStocks,
		NotEnoughUserStocks
	}

	public static string GetDesc(int code)
	{
		return (Code)code switch
		{
			Code.OK => "OK",
			Code.NullParameter => "Cannot process request with null parameter.",
			Code.CantFindUser => "Cannot find user given ID.",
			Code.CantFindStock => "Cannot find stock with given ID.",
			Code.AmountNotPositive => "Amount must be positive.",
			Code.NotEnoughPublicStocks => "Not enough available publically offerred stocks.",
			Code.NotEnoughUserStocks => "Seller does not have enough stocks.",
			_ => "Generic error",
		};
	}

	public static string GetObjectDump(object obj)
	{
		if (obj == null) return "Object was null.";

		var properties = obj.GetType().GetProperties()
			.Select(x => $"{x.Name} was {x.GetValue(obj)}");

		return string.Join(", ", properties);
	}
}
