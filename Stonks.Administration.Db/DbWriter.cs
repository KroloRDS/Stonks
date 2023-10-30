using Stonks.Common.Db;

namespace Stonks.Administration.Db;

public interface IDbWriter
{
	Task<int> SaveChanges(CancellationToken cancellationToken = default);
	DbTransaction BeginTransaction();
	Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default);

	void RollbackTransaction(DbTransaction transaction);
}

public class DbWriter : IDbWriter
{
	private readonly AppDbContext _ctx;

	public DbWriter(AppDbContext ctx)
	{
		_ctx = ctx;
	}
	public async Task<int> SaveChanges(CancellationToken cancellationToken = default) =>
		await _ctx.SaveChangesAsync(cancellationToken);

	public DbTransaction BeginTransaction() => _ctx.BeginTransaction();

	public async Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default) =>
		await _ctx.CommitTransaction(transaction, cancellationToken);

	public void RollbackTransaction(DbTransaction transaction) =>
		_ctx.RollbackTransaction(transaction);
}
