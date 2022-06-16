using Stonks.Models;

namespace Stonks.Managers.Common;

public interface ITransactionManager
{
	List<Transaction> GetTransactions(Guid stockId, DateTime? dateTime = null);
}
