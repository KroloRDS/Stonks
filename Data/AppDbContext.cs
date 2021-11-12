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
}