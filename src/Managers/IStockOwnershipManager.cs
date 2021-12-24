using Stonks.DTOs;

namespace Stonks.Managers;

public interface IStockOwnershipManager
{
	void BuyStock(BuyStockCommand? command);
	void RemoveAllOwnershipForStock(Guid? stockId);
	int GetAllOwnedStocksAmount(Guid? stockId);
}
