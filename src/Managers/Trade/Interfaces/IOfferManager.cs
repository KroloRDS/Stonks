using Stonks.Requests.Commands.Trade;

namespace Stonks.Managers.Trade;

public interface IOfferManager
{
	void PlaceOffer(PlaceOfferCommand? command);
	void AcceptOffer(Guid? userId, Guid? offerId);
	void AcceptOffer(Guid? userId, Guid? offerId, int? amount);
	void CancelOffer(Guid? offerId);
}
