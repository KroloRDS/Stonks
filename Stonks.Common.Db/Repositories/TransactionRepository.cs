using Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Common.Db.Repositories;

public interface ITransactionRepository
{
	IEnumerable<Transaction> Get(Guid? stockId = null,
		Guid? userId = null, DateTime? fromDate = null);
}

public class TransactionRepository : ITransactionRepository
{
	private readonly ReadOnlyDbContext _ctx;

	public TransactionRepository(ReadOnlyDbContext ctx)
	{
		_ctx = ctx;
	}

	public IEnumerable<Transaction> Get(
		Guid? stockId = null,
		Guid? userId = null,
		DateTime? fromDate = null)
	{
		var queryStock = (Transaction x) => !stockId.HasValue ||
			x.StockId == stockId;

		var queryUser = (Transaction x) => !userId.HasValue ||
			x.BuyerId == userId || x.SellerId == userId;

		var queryFrom = (Transaction x) => !fromDate.HasValue ||
			x.Timestamp >= fromDate;

		var query = (Transaction x) => queryStock(x) &&
			queryUser(x) && queryFrom(x);

		var transactions = _ctx.Transaction.Where(query)
			.Select(x => new Transaction
			{
				Amount = x.Amount,
				Price = x.Price
			})
			.ToList();
		return transactions;
	}
}
