using Stonks.DTOs;

namespace Stonks.Managers;

public interface IStockManager
{
	void BuyStock(BuyStockDTO? buyStockDTO);
}
