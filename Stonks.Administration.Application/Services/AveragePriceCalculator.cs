using Stonks.Administration.Domain.Models;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Administration.Application.Services;

public class AveragePriceCalculator
{
	public static (decimal, ulong) FromTransactions(
		IEnumerable<Transaction> transactions, AvgPrice? currentPrice)
	{
		var sharesTraded = currentPrice?.SharesTraded ?? 0;
		var priceSum = sharesTraded * currentPrice?.Price ?? decimal.Zero;

		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			sharesTraded += (ulong)transaction.Amount;
		}

		var avgPrice = sharesTraded > 0 ?
			priceSum / sharesTraded : Constants.STOCK_DEFAULT_PRICE;

		return (avgPrice, sharesTraded);
	}
}
