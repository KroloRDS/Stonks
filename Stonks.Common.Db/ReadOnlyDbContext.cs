using Microsoft.EntityFrameworkCore;

namespace Stonks.Common.Db;

public class ReadOnlyDbContext : AppDbContext
{
	public ReadOnlyDbContext(DbContextOptions<AppDbContext> options)
		: base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}

	public new static DbTransaction BeginTransaction() =>
		throw GetReadOnlyException();

	public new static void RollbackTransaction(DbTransaction transaction) =>
		throw GetReadOnlyException();

	public new static Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default) =>
		throw GetReadOnlyException();

	public override int SaveChanges() => throw GetReadOnlyException();

	public override Task<int> SaveChangesAsync(CancellationToken t = default) =>
		throw GetReadOnlyException();

	private static InvalidOperationException GetReadOnlyException() =>
		new("This context is read only");
}
