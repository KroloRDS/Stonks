﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stonks.Models;

namespace Stonks.Data;
public class AppDbContext : IdentityDbContext<User>
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

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

	public T GetById<T>(Guid? id) where T : HasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		var entity = Find<T>(id);

		if (entity is null)
			throw new KeyNotFoundException(nameof(id));

		return entity;
	}

	public async Task<T> GetByIdAsync<T>(Guid? id,
		CancellationToken cancellationToken) where T : HasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		var entity = await FindAsync<T>(id, cancellationToken);

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

	public async Task<Guid> EnsureExistAsync<T>(Guid? id, 
		CancellationToken cancellationToken) where T : HasId
	{
		if (id is null)
			throw new ArgumentNullException(nameof(id));

		if (!await Set<T>().AnyAsync(x => x.Id == id, cancellationToken))
			throw new KeyNotFoundException(nameof(id));

		return id.Value;
	}

	public User GetUser(Guid? userId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		var user = Users.FirstOrDefault(x => x.Id == userId.ToString());
		if (user is null)
			throw new KeyNotFoundException(nameof(userId));

		return user;
	}

	public Guid EnsureUserExist(string? userId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (!Users.Any(x => x.Id == userId))
			throw new KeyNotFoundException(nameof(userId));

		return Guid.Parse(userId);
	}

	public Guid EnsureUserExist(Guid? userId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (!Users.Any(x => x.Id == userId.ToString()))
			throw new KeyNotFoundException(nameof(userId));

		return userId.Value;
	}

	public Share? GetShares(Guid? userId, Guid? stockId)
	{
		if (userId is null)
			throw new ArgumentNullException(nameof(userId));

		if (stockId is null)
			throw new ArgumentNullException(nameof(stockId));

		return Share.FirstOrDefault(x =>
			x.StockId == stockId &&
			x.OwnerId == userId.ToString());
	}
}