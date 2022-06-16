using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Stonks.Models;
using Stonks.Helpers;
using Stonks.Managers.Common;

namespace UnitTests.Managers.Common;

[TestFixture]
public class UpdatePriceManagerTests : ManagerTest
{
	private readonly UpdatePriceManager _manager;

	public UpdatePriceManagerTests()
	{
		_manager = new UpdatePriceManager(_ctx, new TransactionManager(_ctx));
	}

	[Test]
	public void UpdateAveragePrice_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(
			() => _manager.UpdateAveragePriceForOneStock(null));
	}

	[Test]
	public void UpdateAveragePrice_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(
			() => _manager.UpdateAveragePriceForOneStock(Guid.NewGuid()));
	}

	[Test]
	public void UpdateAveragePrice_BankruptStock_ShouldThrow()
	{
		Assert.Throws<BankruptStockException>(
			() => _manager.UpdateAveragePriceForOneStock(AddBankruptStock().Id));
	}

	[Test]
	public void UpdateAveragePrice_NoData()
	{
		//Arrange
		var stockId = AddStock().Id;

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var priceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.AreEqual(0UL, priceRecord.SharesTraded);
		Assert.AreEqual(IUpdatePriceManager.DEFAULT_PRICE, priceRecord.Amount);
	}

	[Test]
	public void UpdateAveragePrice_NoNewTransactions()
	{
		//Arrange
		var sharesTraded = 100;
		var averagePrice = 5M;
		Assert.Positive(sharesTraded);
		Assert.Positive(averagePrice);

		var stockId = AddStock().Id;
		AddCurrentPrice(stockId, (ulong)sharesTraded, averagePrice);

		//Act
		_manager.UpdateAveragePriceForOneStock(stockId);
		var oldPriceRecord = _ctx.AvgPrice.FirstOrDefault();
		var newPriceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.NotNull(oldPriceRecord);
		Assert.NotNull(newPriceRecord);

		Assert.AreNotEqual(newPriceRecord.Created, oldPriceRecord!.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Amount);
		Assert.AreEqual(averagePrice, newPriceRecord.Amount);

		Assert.AreEqual(sharesTraded, oldPriceRecord.SharesTraded);
		Assert.AreEqual(sharesTraded, newPriceRecord.SharesTraded);
	}

	[Test]
	public void UpdateAveragePrice_OnlyNewTransactions()
	{
		//Arrange
		var stockId = AddStock().Id;

		var prices = new (int Amount, decimal Price)[]
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
		var priceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.AreEqual(expectedAmount, priceRecord.SharesTraded);
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

		var prices = new (int Amount, decimal Price)[]
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
		var oldPriceRecord = _ctx.AvgPrice.FirstOrDefault();
		var newPriceRecord = GetCurrentPrice(stockId);

		//Assert
		Assert.NotNull(oldPriceRecord);
		Assert.NotNull(newPriceRecord);

		Assert.AreNotEqual(newPriceRecord.Created, oldPriceRecord!.DateTime);

		Assert.AreEqual(averagePrice, oldPriceRecord.Amount);
		Assert.AreEqual(expectedAverage, newPriceRecord.Amount);

		Assert.AreEqual(amountTraded, oldPriceRecord.SharesTraded);
		Assert.AreEqual(expectedAmount, newPriceRecord.SharesTraded);
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

	private void AddCurrentPrice(Guid stockId, ulong sharesTraded, decimal averagePrice)
	{
		_ctx.Add(new AvgPriceCurrent
		{
			StockId = stockId,
			Created = DateTime.Now,
			SharesTraded = sharesTraded,
			Amount = averagePrice,
		});
		_ctx.SaveChanges();
	}

	private AvgPriceCurrent GetCurrentPrice(Guid stockId)
	{
		return _ctx.AvgPriceCurrent.Single(x => x.StockId == stockId);
	}
}
