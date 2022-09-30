using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Responses.Bankruptcy;
using Stonks.Requests.Queries.Bankruptcy;

namespace UnitTests.Handlers.Queries.Bankruptcy;

public class GetPublicStockAmountTest :
	QueryTest<GetPublicStocksAmountQuery, GetPublicStocksAmountResponse>
{
	protected override IRequestHandler<GetPublicStocksAmountQuery,
		GetPublicStocksAmountResponse> GetHandler()
	{
		return new GetPublicStocksAmountQueryHandler(_ctx);
	}

	[Test]
	[TestCase(default)]
	[TestCase("be16f8b0-b3c1-48c7-bc97-55449b7ffa3a")]
	public void GetPublicStocksAmount_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(new GetPublicStocksAmountQuery(id));
	}

	[Test]
	public void GetPublicStocksAmount_PositiveTest()
	{
		var amount = 100;
		Assert.Positive(amount);

		var id = AddStock(amount).Id;
		Assert.AreEqual(amount, Handle(
			new GetPublicStocksAmountQuery(id)).Amount);
	}
}
