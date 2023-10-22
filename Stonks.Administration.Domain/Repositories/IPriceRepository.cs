using Stonks.Administration.Domain.Models;

namespace Stonks.Administration.Domain.Repositories;

public interface IPriceRepository
{
	Task<AvgPrice> Current(Guid stockId);
	IEnumerable<AvgPrice> Prices(Guid? stockId = null,
		DateTime? fromDate = null, DateTime? toDate = null);
	Task Add(AvgPrice price, CancellationToken cancellationToken = default);
}
