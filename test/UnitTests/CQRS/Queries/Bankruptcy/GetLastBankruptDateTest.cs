﻿using System;
using MediatR;
using NUnit.Framework;
using Stonks.CQRS.Queries.Bankruptcy;

namespace UnitTests.CQRS.Queries.Bankruptcy;

public class GetLastBankruptDateTest :
	QueryTest<GetLastBankruptDateQuery, GetLastBankruptDateResponse>
{
	protected override IRequestHandler<GetLastBankruptDateQuery,
		GetLastBankruptDateResponse> GetHandler()
	{
		return new GetLastBankruptDateQueryHandler(_readOnlyCtx);
	}

	[Test]
	public void GetBankruptDate_NoStocks_ShouldReturnNull()
	{
		Assert.Null(Handle(new GetLastBankruptDateQuery()).DateTime);
	}

	[Test]
	public void GetBankruptDate_NoBankrupts_ShouldReturnNull()
	{
		AddStock();
		Assert.Null(Handle(new GetLastBankruptDateQuery()).DateTime);
	}

	[Test]
	public void GetBankruptDate_OneBankrupt_ShouldReturn()
	{
		//Arrange
		var stock = AddBankruptStock();
		var date = new DateTime(2012, 6, 5);
		stock.BankruptDate = date;
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(date, Handle(new GetLastBankruptDateQuery()).DateTime);
	}

	[Test]
	public void GetBankruptDate_TwoBankrupts_ShouldReturnNewer()
	{
		//Arange
		AddStock();
		AddStock();
		var stock1 = AddBankruptStock();
		var stock2 = AddBankruptStock();

		var date1 = new DateTime(2012, 6, 5);
		var date2 = new DateTime(2022, 6, 5);
		Assert.Greater(date2, date1);

		stock1.BankruptDate = date1;
		stock2.BankruptDate = date2;
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(date2, Handle(new GetLastBankruptDateQuery()).DateTime);
	}
}
