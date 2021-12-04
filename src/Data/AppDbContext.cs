using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stonks.Models;

namespace Stonks.Data;
public class AppDbContext : IdentityDbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	public DbSet<HistoricalPrice> HistoricalPrice { get; set; }
	public DbSet<Log> Log { get; set; }
	public DbSet<Stock> Stock { get; set; }
	public DbSet<StockOwnership> StockOwnership { get; set; }
	public DbSet<TradeOffer> TradeOffer { get; set; }
	public DbSet<Transaction> Transaction { get; set; }

	public T GetById<T>(Guid? id) where T : HasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		var entity = Find<T>(id);

		if (entity is null)
			throw new KeyNotFoundException(nameof(id));

		return entity;
	}

	public Guid EnsureExist<T>(Guid? id) where T : HasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		if (!Set<T>().Any(x => x.Id == id))
			throw new KeyNotFoundException(nameof(id));

		return id.Value;
	}

	public IdentityUser GetUser(Guid? userId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		var user = Users.FirstOrDefault(x => x.Id == userId.ToString());
		if (user is null)
			throw new KeyNotFoundException(nameof(userId));

		return user;
	}

	public string EnsureUserExist(Guid? userId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (!Users.Any(x => x.Id == userId.ToString()))
			throw new KeyNotFoundException(nameof(userId));

		return userId.Value.ToString();
	}

	public StockOwnership? GetStockOwnership(string? userId, Guid? stockId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));

		return StockOwnership.FirstOrDefault(x =>
			x.StockId == stockId &&
			x.OwnerId == userId);
	}
}