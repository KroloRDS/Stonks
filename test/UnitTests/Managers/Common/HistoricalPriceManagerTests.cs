using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.Models;
using Stonks.Helpers;
using Stonks.Managers.Common;

namespace UnitTests.Managers;

[TestFixture]
public class HistoricalPriceManagerTests : ManagerTest
{
	private readonly PriceManager _manager;

	public HistoricalPriceManagerTests()
	{
		_manager = new PriceManager(_ctx);
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
		Assert.Throws<BankruptStockException>(() => _manager.GetCurrentPrice(stockId));
		Assert.Throws<BankruptStockException>(() => _manager.UpdateAveragePriceForOneStock(stockId));
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
		Assert.AreEqual(decimal.Zero, priceRecord.Amount);
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
		var oldPriceRecord = _ctx.GetById<AvgPrice>(oldPriceId);
		var newPriceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.False(oldPriceRecord.IsCurrent);
		Assert.True(newPriceRecord.IsCurrent);

		Assert.AreNotEqual(newPriceRecord.Id, oldPriceRecord.Id);
		Assert.AreNotEqual(newPriceRecord.DateTime, oldPriceRecord.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Amount);
		Assert.AreEqual(newPriceRecord.Amount, oldPriceRecord.Amount);

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
		Assert.AreEqual(expectedAverage, priceRecord.Amount);
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
		var oldPriceRecord = _ctx.GetById<AvgPrice>(oldPriceId);
		var newPriceRecord = _manager.GetCurrentPrice(stockId);

		//Assert
		Assert.False(oldPriceRecord.IsCurrent);
		Assert.True(newPriceRecord.IsCurrent);

		Assert.AreNotEqual(newPriceRecord.Id, oldPriceRecord.Id);
		Assert.AreNotEqual(newPriceRecord.DateTime, oldPriceRecord.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Amount);
		Assert.AreEqual(expectedAverage, newPriceRecord.Amount);

		Assert.AreEqual(amountTraded, oldPriceRecord.TotalAmountTraded);
		Assert.AreEqual(expectedAmount, newPriceRecord.TotalAmountTraded);
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

	private Guid AddTransaction(Guid stockId, int amount, decimal price)
	{
		var transaction = new Transaction
		{
			StockId = stockId,
			Amount = amount,
			Price = price,
			Timestamp = DateTime.Now,
			BuyerId = AddUser().Id
		};
		_ctx.Add(transaction);
		_ctx.SaveChanges();
		return transaction.Id;
	}

	private Guid AddHistoricalPrice(Guid stockId, ulong amountTraded, decimal averagePrice)
	{
		var price = new AvgPrice
		{
			StockId = stockId,
			DateTime = DateTime.Now,
			IsCurrent = true,
			TotalAmountTraded = amountTraded,
			Amount = averagePrice,
			AmountNormalised = averagePrice
		};
		_ctx.Add(price);
		_ctx.SaveChanges();
		return price.Id;
	}
}
