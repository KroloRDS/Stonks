using System.Linq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Models;

namespace UnitTests;

public class UsingContext
{
	protected readonly AppDbContext _ctx;

	public UsingContext()
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
}