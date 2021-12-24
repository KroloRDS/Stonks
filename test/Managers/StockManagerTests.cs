using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

using Stonks.Managers;

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
		Assert.NotNull(stock.BankruptDate);
	}
}
