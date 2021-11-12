using Stonks.DTOs;

namespace Stonks.Managers;

public interface IStockManager
{
	int BuyStockFromUser(BuyStockDTO buyStockDTO);
	int BuyPublicallyOfferredStock(BuyStockDTO buyStockDTO);
}
