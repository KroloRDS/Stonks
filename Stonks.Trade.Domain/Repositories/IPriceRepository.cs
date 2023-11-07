using Stonks.Trade.Domain.Models;

namespace Stonks.Trade.Domain.Repositories;

public interface IPriceRepository
{
	Task<decimal?> Current(Guid stockId);
	IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null);
}
