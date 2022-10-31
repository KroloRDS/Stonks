using System;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Data.Models;

namespace UnitTests;

public class InMemoryDb
{
	protected readonly AppDbContext _ctx;

	public InMemoryDb()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: "FakeDbContext")
			.EnableSensitiveDataLogging()
			.Options;

		_ctx = new AppDbContext(options);
	}

	[TearDown]
	public void TearDown()
	{
		_ctx.Database.EnsureDeleted();
		_ctx.ChangeTracker.Clear();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		_ctx.Dispose();
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

	protected User AddUser()
	{
		var user = new User();
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
			WriterId = type == OfferType.PublicOfferring ? null : AddUser().Id
		};
		_ctx.Add(offer);
		_ctx.SaveChanges();
		return offer;
	}
}
