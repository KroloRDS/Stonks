using Stonks.Trade.Domain.Models;

namespace Stonks.Trade.Domain.Repositories;

public interface ITransactionRepository
{
	Task AddLog(Transaction transaction, CancellationToken cancellationToken);
}
