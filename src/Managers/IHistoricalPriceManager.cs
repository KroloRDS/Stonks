using Stonks.Models;

namespace Stonks.Managers;

public interface IHistoricalPriceManager
{
	HistoricalPrice GetCurrentPrice(Guid? stockId);
	void UpdateAveragePrices();
	void UpdateAveragePriceForOneStock(Guid stockId);
}