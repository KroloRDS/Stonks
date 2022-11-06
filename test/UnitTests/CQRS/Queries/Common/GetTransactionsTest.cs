﻿using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using NUnit.Framework;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace UnitTests.CQRS.Queries.Common;

public class GetTransactionsTest
	: QueryTest<GetTransactionsQuery, GetTransactionsResponse>
{
	protected override IRequestHandler<GetTransactionsQuery,
		GetTransactionsResponse> GetHandler()
	{
		return new GetTransactionsQueryHandler(_readOnlyCtx);
	}

	[Test]
	[TestCase(default)]
	[TestCase(_zeroGuid)]
	[TestCase(_randomGuid)]
	public void GetTransactions_WrongStock_ShouldThrow(Guid id)
	{
		AssertThrows<KeyNotFoundException>(new GetTransactionsQuery(id));
	}

	[Test]
	public void GetTransactions_NoTransactions_ShouldReturnEmptyList()
	{
		Assert.False(Handle(new GetTransactionsQuery(AddStock().Id))
			.Transactions.Any());
	}

	[Test]
	public void GetTransactions_PositiveTest()
	{
		//Arrange
		var date1 = new DateTime(2001, 1, 1);
		var date2 = new DateTime(2002, 1, 1);
		Assert.Greater(date2, date1);

		//Arrange
		var stockId = AddStock().Id;
		var buyerId = AddUser().Id;
		_ctx.AddRange(new[]
		{
			new Transaction
			{
				StockId = stockId,
				BuyerId = buyerId,
				Timestamp = date1
			},
			new Transaction
			{
				StockId = stockId,
				BuyerId = buyerId,
				Timestamp = date2
			}
		});
		_ctx.SaveChanges();

		//Assert
		Assert.AreEqual(2, Handle(new GetTransactionsQuery(stockId))
			.Transactions.Count());
		Assert.AreEqual(1, Handle(new GetTransactionsQuery(stockId, date2))
			.Transactions.Count());
		Assert.False(Handle(new GetTransactionsQuery(stockId, date2.AddDays(1)))
			.Transactions.Any());
	}
}
