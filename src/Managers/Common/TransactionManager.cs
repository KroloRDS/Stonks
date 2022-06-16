using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers.Common;

public class TransactionManager : ITransactionManager
{
	private readonly AppDbContext _ctx;

	public TransactionManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public List<Transaction> GetTransactions(
		Guid stockId, DateTime? dateTime = null)
	{
		var query = _ctx.Transaction
			.Where(x => x.StockId == stockId);

		if (dateTime is not null)
		{
			query = _ctx.Transaction
				.Where(x => x.StockId == stockId && x.Timestamp >= dateTime);
		}

		return query.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToList();
	}
}
