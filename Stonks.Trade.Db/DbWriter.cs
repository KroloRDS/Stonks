using Stonks.Common.Db;

namespace Stonks.Trade.Db;

public interface IDbWriter
{
	Task<int> SaveChanges(CancellationToken cancellationToken = default);
	DbTransaction BeginTransaction();
	Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default);
}

public class DbWriter : IDbWriter
{
	private readonly AppDbContext _ctx;

	public DbWriter(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public DbTransaction BeginTransaction() => _ctx.BeginTransaction();

	public async Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default) =>
		await _ctx.CommitTransaction(transaction, cancellationToken);

	public async Task<int> SaveChanges(CancellationToken cancellationToken = default) =>
		await _ctx.SaveChangesAsync(cancellationToken);
}
