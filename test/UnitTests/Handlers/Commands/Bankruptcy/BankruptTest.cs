using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class BankruptTest : CommandTest<BankruptCommand>
{
	protected override IRequestHandler<BankruptCommand, Unit> GetHandler()
	{
		return new BankruptCommandHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("8bc61de3-1cc0-45f7-81a4-75c5588c3f16")]
	public void BankruptStock_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(new BankruptCommand(id));
	}

	[Test]
	public void BankruptStock_PositiveTest()
	{
		var stock = AddStock();
		Handle(new BankruptCommand(stock.Id));
		Assert.IsTrue(stock.Bankrupt);
		Assert.Zero(stock.PublicallyOfferredAmount);
		Assert.NotNull(stock.BankruptDate);
	}
}
