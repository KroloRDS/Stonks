using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stonks.Data.Models;
using Stonks.Util;

namespace Stonks.Data;
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
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
	public DbSet<AvgPriceCurrent> AvgPriceCurrent { get; set; }
	public DbSet<Log> Log { get; set; }
	public DbSet<Stock> Stock { get; set; }
	public DbSet<Share> Share { get; set; }
	public DbSet<TradeOffer> TradeOffer { get; set; }
	public DbSet<Transaction> Transaction { get; set; }

	public async Task<T> GetById<T>(Guid? id) where T : class, IHasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		var entity = await FindAsync<T>(id);

		if (entity is null)
			throw new KeyNotFoundException(nameof(id));

		return entity;
	}

	public async Task<Guid> EnsureExist<T>(Guid? id,
		CancellationToken cancellationToken) where T : class, IHasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		if (!await Set<T>().AnyAsync(x => x.Id == id, cancellationToken))
			throw new KeyNotFoundException(nameof(id));

		return id.Value;
	}

	public async Task<Share?> GetShares(Guid? userId,
		Guid? stockId, CancellationToken cancellationToken)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));

		return await Share.FirstOrDefaultAsync(x =>
			x.StockId == stockId && x.OwnerId == userId,
			cancellationToken);
	}

	public virtual async Task ExecuteTransaction(Task task,
		string handlerName, CancellationToken cancellationToken)
	{
		try
		{
			await task;
			await base.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			ChangeTracker.Clear();
			var logged = new StonksLogger(this).Log(ex);
			throw new DbTransactionException(handlerName, logged, ex);
		}
	}
}