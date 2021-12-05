using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Managers;

namespace UnitTests.Managers;

[TestFixture]
public class HistoricalPriceManagerTests : ManagerTest
{
	private readonly HistoricalPriceManager _manager;

	public HistoricalPriceManagerTests()
	{
		_manager = new HistoricalPriceManager(_ctx);
	}

	[Test]
	public void GetAndUpdateAveragePrice_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.GetCurrentPrice(null));
		Assert.Throws<ArgumentNullException>(() => _manager.UpdateAveragePriceForOneStock(null));
	}

	[Test]
	public void GetAndUpdateAveragePrice_WrongStock_ShouldThrow()
	{
		var stockId = Guid.NewGuid();
		Assert.Throws<KeyNotFoundException>(() => _manager.GetCurrentPrice(stockId));
		Assert.Throws<KeyNotFoundException>(() => _manager.UpdateAveragePriceForOneStock(stockId));
	}

	[Test]
	public void GetAndUpdateAveragePrice_BankruptStock_ShouldThrow()
	{
		var stockId = AddBankruptStock().Id;
		Assert.Throws<InvalidOperationException>(() => _manager.GetCurrentPrice(stockId));
		Assert.Throws<InvalidOperationException>(() => _manager.UpdateAveragePriceForOneStock(stockId));
	}

	[Test]
	public void UpdateAveragePrice_NoData()
	{
		//Arrange
		var stockId = AddStock().Id;

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var priceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.True(priceRecord.IsCurrent);
		Assert.AreEqual(0UL, priceRecord.TotalAmountTraded);
		Assert.AreEqual(decimal.Zero, priceRecord.AveragePrice);
	}

	[Test]
	public void UpdateAveragePrice_NoNewTransactions()
	{
		//Arrange
		var amountTraded = 100;
		var averagePrice = 5M;
		Assert.Positive(amountTraded);
		Assert.Positive(averagePrice);

		var stockId = AddStock().Id;
		var oldPriceId = AddHistoricalPrice(stockId, (ulong)amountTraded, averagePrice);

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var oldPriceRecord = _ctx.GetById<HistoricalPrice>(oldPriceId);
		var newPriceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.False(oldPriceRecord.IsCurrent);
		Assert.True(newPriceRecord.IsCurrent);

		Assert.AreNotEqual(newPriceRecord.Id, oldPriceRecord.Id);
		Assert.AreNotEqual(newPriceRecord.DateTime, oldPriceRecord.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.AveragePrice);
		Assert.AreEqual(newPriceRecord.AveragePrice, oldPriceRecord.AveragePrice);

		Assert.AreEqual(amountTraded, oldPriceRecord.TotalAmountTraded);
		Assert.AreEqual(newPriceRecord.TotalAmountTraded, oldPriceRecord.TotalAmountTraded);
	}

	[Test]
	public void UpdateAveragePrice_OnlyNewTransactions()
	{
		//Arrange
		var stockId = AddStock().Id;

		var prices = new List<(int Amount, decimal Price)>
		{
			new (10, 10M),
			new (20, 20M),
			new (30, 30M),
			new (11, 10M)
		};

		foreach ((var amount, var price) in prices)
		{
			Assert.Positive(amount);
			Assert.Positive(price);
			AddTransaction(stockId, amount, price);
		}

		var expectedAmount = prices.Sum(x => x.Amount);
		var expectedAverage = prices.Sum(x => x.Amount * x.Price) / expectedAmount;

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var priceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.True(priceRecord.IsCurrent);
		Assert.AreEqual(expectedAmount, priceRecord.TotalAmountTraded);
		Assert.AreEqual(expectedAverage, priceRecord.AveragePrice);
	}

	[Test]
	public void UpdateAveragePrice_OldAndNewTransactions()
	{
		//Arrange
		var stockId = AddStock().Id;
		var amountTraded = 100;
		var averagePrice = 5M;
		Assert.Positive(amountTraded);
		Assert.Positive(averagePrice);

		AddTransaction(stockId, amountTraded, averagePrice);
		_manager.UpdateAveragePriceForOneStock(stockId);
		var oldPriceId = _manager.GetCurrentPrice(stockId).Id;

		var prices = new List<(int Amount, decimal Price)>
		{
			new (10, 10M),
			new (20, 20M),
			new (30, 30M),
			new (11, 10M)
		};

		foreach ((var amount, var price) in prices)
		{
			Assert.Positive(amount);
			Assert.Positive(price);
			AddTransaction(stockId, amount, price);
		}

		var expectedAmount = prices.Sum(x => x.Amount) + amountTraded;
		var expectedAverage = (prices.Sum(x => x.Amount * x.Price) +
			amountTraded * averagePrice) / expectedAmount;

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var oldPriceRecord = _ctx.GetById<HistoricalPrice>(oldPriceId);
		var newPriceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.False(oldPriceRecord.IsCurrent);
		Assert.True(newPriceRecord.IsCurrent);

		Assert.AreNotEqual(newPriceRecord.Id, oldPriceRecord.Id);
		Assert.AreNotEqual(newPriceRecord.DateTime, oldPriceRecord.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.AveragePrice);
		Assert.AreEqual(expectedAverage, newPriceRecord.AveragePrice);

		Assert.AreEqual(amountTraded, oldPriceRecord.TotalAmountTraded);
		Assert.AreEqual(expectedAmount, newPriceRecord.TotalAmountTraded);
	}
}
