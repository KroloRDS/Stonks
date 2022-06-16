using Stonks.Models;

namespace Stonks.Managers.Common;

public interface ITransactionManager
{
	IEnumerable<Transaction> GetTransactions(Guid stockId, DateTime? dateTime = null);
}
