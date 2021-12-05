using Stonks.Models;

namespace Stonks.Managers;

public interface IHistoricalPriceManager
{
	HistoricalPrice GetCurrentPrice(Guid? stockId);
	List<HistoricalPrice> GetHistoricalPrices(Guid? stockId, DateTime? fromDate, DateTime? toDate);
	void UpdateAveragePrices();
	void UpdateAveragePriceForOneStock(Guid? stockId);
}