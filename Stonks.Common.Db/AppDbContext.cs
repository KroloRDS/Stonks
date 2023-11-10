using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Stonks.Common.Db.EntityFrameworkModels;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Common.Db;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options) {}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.Entity<Share>()
			.HasKey(x => new { x.OwnerId, x.StockId });
	}

	public DbSet<AvgPrice> AvgPrice { get; set; }
	public DbSet<Log> Log { get; set; }
	public DbSet<Stock> Stock { get; set; }
	public DbSet<Share> Share { get; set; }
	public DbSet<TradeOffer> TradeOffer { get; set; }
	public DbSet<Transaction> Transaction { get; set; }
	public DbSet<User> User { get; set; }

	public async Task<T?> GetById<T>(Guid? id) where T : class, IHasId
	{
		if (id is null) return null;
		return await FindAsync<T>(id);
	}

	public async Task<bool> EnsureExist<T>(Guid? id,
		CancellationToken cancellationToken = default) where T : class, IHasId
	{
		return id is not null &&
			(await Set<T>().AnyAsync(x => x.Id == id, cancellationToken));
	}

	public async Task<Share?> GetShares(Guid? userId, Guid? stockId,
		CancellationToken cancellationToken = default)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));

		return await Share.FirstOrDefaultAsync(x =>
			x.StockId == stockId && x.OwnerId == userId,
			cancellationToken);
	}

	public DbTransaction BeginTransaction() => new DbTransaction();

	public void RollbackTransaction(DbTransaction transaction) =>
		ChangeTracker.Clear();

	public async Task CommitTransaction(DbTransaction transaction,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await base.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			RollbackTransaction(transaction);
			throw new DbTransactionException(ex);
		}
	}
}