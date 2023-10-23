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

	public DbTransaction BeginTransaction() =>
		throw GetReadOnlyException();

	public void RollbackTransaction(DbTransaction transaction) =>
		throw GetReadOnlyException();

	public async Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default) =>
		throw GetReadOnlyException();

	public override int SaveChanges() => throw GetReadOnlyException();

	public override Task<int> SaveChangesAsync(CancellationToken t) =>
		throw GetReadOnlyException();

	private static InvalidOperationException GetReadOnlyException() =>
		new("This context is read only");
}
