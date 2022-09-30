using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Models;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class RemoveAllOffersForStockTest :
	CommandTest<RemoveAllOffersForStockCommand>
{
	protected override IRequestHandler<RemoveAllOffersForStockCommand, Unit>
		GetHandler()
	{
		return new RemoveAllOffersForStockCommandHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("209a450e-142c-4d64-a073-2783862e0b64")]
	public void RemoveAllOffersForStock_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(
			new RemoveAllOffersForStockCommand(id));
	}

	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(14)]
	public void RemoveAllOffersForStock_PositiveTest(int offerCount)
	{
		//Arange
		var stockId = AddStock().Id;
		var offers = Enumerable.Range(1, offerCount)
			.Select(x => new TradeOffer { StockId = stockId });
		_ctx.AddRange(offers);
		_ctx.SaveChanges();
		Assert.AreEqual(offerCount > 0, _ctx.TradeOffer.Any());

		//Act
		Handle(new RemoveAllOffersForStockCommand(stockId));

		//Assert
		Assert.False(_ctx.TradeOffer.Any());
	}
}
