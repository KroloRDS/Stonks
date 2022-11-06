﻿using System;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.CQRS.Queries.Bankruptcy;

namespace UnitTests.CQRS.Queries.Bankruptcy;

public class GetPublicStockAmountTest :
	QueryTest<GetPublicStocksAmountQuery, GetPublicStocksAmountResponse>
{
	protected override IRequestHandler<GetPublicStocksAmountQuery,
		GetPublicStocksAmountResponse> GetHandler()
	{
		return new GetPublicStocksAmountQueryHandler(_readOnlyCtx);
	}

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
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
