using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Models;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class RemoveAllSharesTest : CommandTest<RemoveAllSharesCommand>
{
	protected override IRequestHandler<RemoveAllSharesCommand, Unit>
		GetHandler()
	{
		return new RemoveAllSharesCommandHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("209a450e-142c-4d64-a073-2783862e0b64")]
	public void RemoveAllShares_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(
			new RemoveAllSharesCommand(id));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllShares_PositiveTest(int sharesCount)
	{
		//Arange
		var stockId = AddStock().Id;
		var offers = Enumerable.Range(1, sharesCount)
			.Select(x => new Share
			{
				StockId = stockId,
				OwnerId = Guid.NewGuid().ToString()
			});
		_ctx.AddRange(offers);
		_ctx.SaveChanges();
		Assert.AreEqual(sharesCount > 0, _ctx.Share.Any());

		//Act
		Handle(new RemoveAllSharesCommand(stockId));

		//Assert
		Assert.False(_ctx.Share.Any());
	}
}
