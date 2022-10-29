using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stonks.Models;
using Stonks.Providers;

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

	public async Task<Guid> EnsureExistAsync<T>(Guid? id, 
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

	public async Task ExecuteTransactionAsync(Task task,
		string handlerName, CancellationToken cancellationToken)
	{
		try
		{
			await task;
			await SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			ChangeTracker.Clear();
			throw GetTransactionException(handlerName, ex);
		}
	}

	private Exception GetTransactionException(
		string handlerName, Exception inner)
	{
		var msg = $"Exception during transaction in {handlerName}. ";
		try
		{
			new StonksLogger(this).Log(inner);
			msg += "See inner exception for details.";
		}
		catch
		{
			msg += "Failed to log inner exception.";
		}
		return new Exception(msg, inner);
	}
}