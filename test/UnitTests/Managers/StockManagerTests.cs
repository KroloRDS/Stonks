using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

using Stonks.Managers;
using Stonks.Models;

namespace UnitTests.Managers;

[TestFixture]
public class StockManagerTests : ManagerTest
{
	private readonly StockManager _manager;

	public StockManagerTests()
	{
		var mockTradeManager = new Mock<ITradeManager>();
		var mockOwnershipManager = new Mock<IStockOwnershipManager>();
		_manager = new StockManager(_ctx, mockOwnershipManager.Object, mockTradeManager.Object);
	}

	[Test]
	public void BankruptStock_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.Bankrupt(null));
	}

	[Test]
	public void BankruptStock_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(() => _manager.Bankrupt(Guid.NewGuid()));
	}

	[Test]
	public void BankruptStock_PositiveTest()
	{
		var stock = AddStock();
		_manager.Bankrupt(stock.Id);
		Assert.IsTrue(stock.Bankrupt);
		Assert.Zero(stock.PublicallyOfferredAmount);
		Assert.NotNull(stock.BankruptDate);
	}

	[Test]
	public void GetBankruptDate_NoStocks_ShouldReturnNull()
	{
		Assert.Null(_manager.GetLastBankruptDate());
	}

	[Test]
	public void GetBankruptDate_NoBankrupts_ShouldReturnNull()
	{
		AddStock();
		Assert.Null(_manager.GetLastBankruptDate());
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
		Assert.AreEqual(date, _manager.GetLastBankruptDate());
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
		Assert.AreEqual(date2, _manager.GetLastBankruptDate());
	}

	[Test]
	public void GetPublicStocksAmount_NullStock_ShouldThrow()
	{
		Assert.Throws<ArgumentNullException>(() => _manager.GetPublicStocksAmount(null));
	}

	[Test]
	public void GetPublicStocksAmount_WrongStock_ShouldThrow()
	{
		Assert.Throws<KeyNotFoundException>(() => _manager.GetPublicStocksAmount(Guid.NewGuid()));
	}

	[Test]
	public void GetPublicStocksAmount_PositiveTest()
	{
		var amount = 100;
		Assert.Positive(amount);

		var id = AddStock(amount).Id;
		Assert.AreEqual(amount, _manager.GetPublicStocksAmount(id));
	}

	[Test]
	public void EmitNewStocks()
	{
		//Arrange
		var amount = 100;
		var smallerAmount = 50;
		var biggerAmount = 150;

		//Check if test data makes sense
		Assert.Positive(amount);
		Assert.Positive(smallerAmount);
		Assert.Positive(biggerAmount);
		Assert.Greater(biggerAmount, amount);
		Assert.Greater(amount, smallerAmount);

		var stock1 = AddStock(0);
		var stock2 = AddStock(smallerAmount);
		var stock3 = AddStock(biggerAmount);
		var bankruptStock = AddBankruptStock();

		//Act
		_manager.EmitNewStocks(amount);

		//Assert
		Assert.AreEqual(amount, GetPublicAmount(stock1.Id));
		Assert.AreEqual(amount, GetPublicAmount(stock2.Id));
		Assert.AreEqual(biggerAmount, GetPublicAmount(stock3.Id));
		Assert.Zero(GetPublicAmount(bankruptStock.Id));
	}

	private int GetPublicAmount(Guid stockId)
	{
		return _ctx.GetById<Stock>(stockId).PublicallyOfferredAmount;
	}
}
