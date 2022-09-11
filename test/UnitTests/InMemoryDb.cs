using System;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Models;


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

	protected static Guid GetUserId(User user)
	{
		return Guid.Parse(user.Id);
	}
}
