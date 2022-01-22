using Stonks.DTOs;

namespace Stonks.Managers;

public interface ITradeManager
{
	void PlaceOffer(PlaceOfferCommand? command);
	void AcceptOffer(Guid? userId, Guid? offerId);
	void AcceptOffer(Guid? userId, Guid? offerId, int? amount);
	void RemoveOffer(Guid? offerId);
	void RemoveAllOffersForStock(Guid? stockId);
	void AddPublicOffers(int amount);
}
