using Stonks.Administration.Domain.Models;

namespace Stonks.Administration.Domain.Repositories;

public interface ITransactionRepository
{
	IEnumerable<Transaction> Get(Guid? stockId = null,
		Guid? userId = null, DateTime? fromDate = null);
}
