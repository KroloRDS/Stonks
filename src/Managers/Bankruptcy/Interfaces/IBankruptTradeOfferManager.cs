namespace Stonks.Managers.Bankruptcy;

public interface IBankruptTradeOfferManager
{
	void RemoveAllOffersForStock(Guid? stockId);
	void AddPublicOffers(int amount);
}
