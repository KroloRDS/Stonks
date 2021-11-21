using System;
using System.Linq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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
		Assert.AreEqual(0, _ctx.Log.Count());

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
			Price = 1M,
			PublicallyOfferredAmount = publicAmount
		};
		_ctx.Add(stock);
		_ctx.SaveChanges();
		return stock;
	}

	protected IdentityUser AddUser()
	{
		var user = new IdentityUser();
		_ctx.Add(user);
		_ctx.SaveChanges();
		return user;
	}

	protected static Guid GetUserId(IdentityUser user)
	{
		return Guid.Parse(user.Id);
	}
}