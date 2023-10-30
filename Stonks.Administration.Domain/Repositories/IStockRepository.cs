using Stonks.Administration.Domain.Models;

namespace Stonks.Administration.Domain.Repositories;

public interface IStockRepository
{
	Task<Stock?> Get(Guid stockId);
	Task<IEnumerable<Stock>> GetActive(
		CancellationToken cancellationToken = default);
	Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default);

	Task Bankrupt(Guid stockId);
}
