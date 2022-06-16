using Stonks.Models;

namespace Stonks.Managers.Common;

public interface IGetPriceManager
{
	AvgPriceCurrent GetCurrentPrice(Guid? stockId);
	IEnumerable<AvgPrice> GetHistoricalPrices(
		Guid? stockId, DateTime? fromDate, DateTime? toDate = null);
}