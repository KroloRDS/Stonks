using System.Linq;
using NUnit.Framework;
using Stonks.Data.Models;

namespace UnitTests;

public class InMemoryDbTest : InMemoryDb
{
	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(4)]
	public void TestFakeContext(int count)
	{
		Assert.False(_ctx.Log.Any());
		var logs = Enumerable.Range(1, count)
			.Select(x => new Log { Id = x });
		_ctx.AddRange(logs);
		_ctx.SaveChanges();
		Assert.AreEqual(count, _ctx.Log.Count());
	}
}
