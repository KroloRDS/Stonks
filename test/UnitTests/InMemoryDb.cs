using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Stonks.Data;
using Stonks.Data.Models;

namespace UnitTests;

public class InMemoryDb
{
	protected readonly AppDbContext _ctx;
	protected readonly ReadOnlyDbContext _readOnlyCtx;

	public InMemoryDb()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase("FakeDbContext", new InMemoryDatabaseRoot())
			.EnableSensitiveDataLogging()
			.Options;

		_ctx = new AppDbContext(options);
		_readOnlyCtx = new ReadOnlyDbContext(options);
	}

	[TearDown]
	public void TearDown()
	{
		_ctx.ChangeTracker.Clear();
		_ctx.Database.EnsureDeleted();
		_readOnlyCtx.Database.EnsureDeleted();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		_ctx.Dispose();
		_readOnlyCtx.Dispose();
	}

	protected Stock AddStock(int publicAmount = 100)
	{
		var stock = new Stock
		{
			Symbol = "TEST",
			Name = "TestStock",
			PublicallyOfferredAmount = publicAmount
		};
		_ctx.Add(stock);
		_ctx.SaveChanges();
		return stock;
	}

	protected Stock AddBankruptStock()
	{
		var stock = new Stock
		{
			Symbol = "TEST",
			Name = "TestStock",
			Bankrupt = true
		};
		_ctx.Add(stock);
		_ctx.SaveChanges();
		return stock;
	}

	protected User AddUser(decimal funds = 0)
	{
		var user = new User { Funds = funds };
		_ctx.Add(user);
		_ctx.SaveChanges();
		return user;
	}

	protected TradeOffer AddOffer(OfferType type)
	{
		var offer = new TradeOffer
		{
			Amount = 10,
			Price = 1M,
			StockId = AddStock().Id,
			Type = type,
			WriterId = type == OfferType.PublicOfferring ?
				null : AddUser(100M).Id
		};
		_ctx.Add(offer);
		_ctx.SaveChanges();
		return offer;
	}
}
