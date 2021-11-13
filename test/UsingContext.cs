using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;

namespace UnitTests;

public class UsingContext
{
	protected AppDbContext ctx;

	[SetUp]
	public void SetUp()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: "FakeDbContext")
			.Options;

		ctx = new AppDbContext(options);
	}

	[TearDown]
	public void TearDown()
	{
		ctx.Database.EnsureDeleted();
	}
}