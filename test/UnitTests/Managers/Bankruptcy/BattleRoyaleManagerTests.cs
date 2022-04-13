using Moq;
using NUnit.Framework;

using Stonks.Helpers;
using Stonks.Managers.Common;
using Stonks.Managers.Bankruptcy;

namespace UnitTests.Managers.BattleRoyale;

[TestFixture]
public class BattleRoyaleManagerTests : ManagerTest
{
	private readonly BattleRoyaleManager _manager;

	public BattleRoyaleManagerTests()
	{
		var mockPriceManager = new Mock<IPriceManager>();
		var mockOwnershipManager = new Mock<IBankruptSharesManager>();
		var mockStockManager = new Mock<IBankruptStockManager>();
		var mockConfigManager = new Mock<IConfigurationManager>();
		
		_manager = new BattleRoyaleManager(_ctx, mockPriceManager.Object,
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
