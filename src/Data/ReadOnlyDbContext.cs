using Microsoft.EntityFrameworkCore;

namespace Stonks.Data;

public class ReadOnlyDbContext : AppDbContext
{
    public ReadOnlyDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

	public override Task ExecuteTransaction(Task t, string mame,
		CancellationToken ct) => throw GetSaveChangesException();

	public override int SaveChanges() => throw GetSaveChangesException();

    public override Task<int> SaveChangesAsync(CancellationToken t) =>
		throw GetSaveChangesException();

    private static InvalidOperationException GetSaveChangesException() =>
        new("This context is read only");
}
