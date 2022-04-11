using Moq;
using NUnit.Framework;
using Stonks.Helpers;
using Stonks.Managers;

namespace UnitTests.Managers;

[TestFixture]
public class BattleRoyaleManagerTests : ManagerTest
{
	private readonly BattleRoyaleManager _manager;

	public BattleRoyaleManagerTests()
	{
		var mockHistoricalManager = new Mock<IHistoricalPriceManager>();
		var mockOwnershipManager = new Mock<IStockOwnershipManager>();
		var mockStockManager = new Mock<IStockManager>();
		var mockConfigManager = new Mock<IConfigurationManager>();
		
		_manager = new BattleRoyaleManager(_ctx, mockHistoricalManager.Object,
			mockOwnershipManager.Object, mockStockManager.Object,
			mockConfigManager.Object);
	}

	[Test]
	public void GetWeakestStockId_NoStocks_ShouldThrow()
	{
		Assert.Throws<NoStocksToBankruptException>(
			() => _manager.GetWeakestStockId());

		AddBankruptStock();

		Assert.Throws<NoStocksToBankruptException>(
			() => _manager.GetWeakestStockId());
	}
}
