using Stonks.Models;

namespace Stonks.Managers.Common;

public interface IPriceManager
{
	AvgPrice GetCurrentPrice(Guid? stockId);
	List<AvgPrice> GetHistoricalPrices(
		Guid? stockId, DateTime? fromDate, DateTime? toDate = null);
	void UpdateAveragePrices();
	void UpdateAveragePriceForOneStock(Guid? stockId);
}