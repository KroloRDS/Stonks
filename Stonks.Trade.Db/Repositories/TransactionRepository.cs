using Stonks.Common.Db;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Db.Repositories;

public class TransactionRepository : ITransactionRepository
{
	private readonly AppDbContext _ctx;

	public TransactionRepository(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task AddLog(Transaction transaction,
		CancellationToken cancellationToken = default) =>
		await _ctx.AddAsync(transaction, cancellationToken);
}
