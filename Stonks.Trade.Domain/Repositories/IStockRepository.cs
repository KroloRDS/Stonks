using Stonks.Trade.Domain.Models;

namespace Stonks.Trade.Domain.Repositories;

public interface IStockRepository
{
	Task<bool> IsBankrupt(Guid stockId);
	Dictionary<Guid, Stock> GetStockNames();
	Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default);
}
