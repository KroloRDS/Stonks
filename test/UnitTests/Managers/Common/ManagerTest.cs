using System;
using System.Linq;

using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Models;

namespace UnitTests;

public class ManagerTest
{
	protected readonly AppDbContext _ctx;

	public ManagerTest()
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

	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(4)]
	public void TestFakeContext(int count)
	{
		Assert.False(_ctx.Log.Any());

		for (int i = 0; i < count; i++)
		{
			_ctx.Add(new Log());
		}
		_ctx.SaveChanges();

		Assert.AreEqual(count, _ctx.Log.Count());
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