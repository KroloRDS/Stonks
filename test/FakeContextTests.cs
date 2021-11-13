using System.Linq;
using NUnit.Framework;
using Stonks.Models;

namespace UnitTests;

[TestFixture]
public class FakeContextTests : UsingContext
{
	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(4)]
	public void TestFakeContext(int count)
	{
		Assert.AreEqual(0, ctx.Log.Count());

		for (int i = 0; i < count; i++)
		{
			ctx.Log.Add(new Log());
		}
		ctx.SaveChanges();

		Assert.AreEqual(count, ctx.Log.Count());
	}
}