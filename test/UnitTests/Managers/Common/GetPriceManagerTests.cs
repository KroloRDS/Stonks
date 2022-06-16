using System;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.Models;
using Stonks.Helpers;
using Stonks.Managers.Common;

namespace UnitTests.Managers.Common;

[TestFixture]
public class GetPriceManagerTests : ManagerTest
{
	private readonly GetPriceManager _manager;

	public GetPriceManagerTests()
	{
		_manager = new GetPriceManager(_ctx);
	}

	[Test]
	public void GetAndUpdateAveragePrice_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.GetCurrentPrice(null));
	}

	[Test]
	public void GetAndUpdateAveragePrice_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.GetCurrentPrice(Guid.NewGuid()));
	}

	[Test]
	public void GetAndUpdateAveragePrice_BankruptStock_ShouldThrow()
	{
		Assert.Throws<BankruptStockException>(
			() => _manager.GetCurrentPrice(AddBankruptStock().Id));
	}

	[Test]
	public void GetHistoricalPrices_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_manager.GetHistoricalPrices(null, new DateTime(2021, 1, 1)));
	}

	[Test]
	public void GetHistoricalPrices_NullDate_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_manager.GetHistoricalPrices(AddStock().Id, null));
	}

	[Test]
	public void GetHistoricalPrices_WrongDate_ShouldThrow()
	{
		var stockId = AddBankruptStock().Id;
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			_manager.GetHistoricalPrices(AddStock().Id,
			new DateTime(2021, 1, 1), new DateTime(2020, 1, 1)));
	}

	[Test]
	public void GetHistoricalPrices_PositiveTest()
	{
		//Arrange
		var stock = AddStock();
		var historicalPrices = new[]
		{
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2020, 1, 1)
			},
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2021, 1, 1)
			},
			new AvgPrice
			{
				Stock = stock,
				DateTime = new DateTime(2022, 1, 1)
			},
		};
		_ctx.AddRange(historicalPrices);
		_ctx.SaveChanges();

		//Act & Assert
		Assert.AreEqual(2, GetPricesCount(stock.Id, new DateTime(2020, 6, 6)));
		Assert.AreEqual(1, GetPricesCount(stock.Id, new DateTime(2022, 1, 1)));
		Assert.AreEqual(1, GetPricesCount(stock.Id, new DateTime(2019, 6, 6), new DateTime(2020, 6, 6)));
		Assert.AreEqual(2, GetPricesCount(stock.Id, new DateTime(2019, 6, 6), new DateTime(2021, 1, 1)));
		Assert.AreEqual(0, GetPricesCount(stock.Id, new DateTime(2021, 6, 6), new DateTime(2021, 7, 7)));
		Assert.AreEqual(0, GetPricesCount(stock.Id, new DateTime(2001, 6, 6), new DateTime(2019, 7, 7)));
		Assert.AreEqual(0, GetPricesCount(stock.Id, new DateTime(2025, 1, 1)));
	}

	private int GetPricesCount(Guid stockId, DateTime from, DateTime? to = null)
	{
		return _manager.GetHistoricalPrices(stockId, from, to).Count;
	}
}
