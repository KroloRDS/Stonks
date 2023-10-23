using Stonks.Trade.Domain.Models;

namespace Stonks.Trade.Domain.Repositories;

public interface IOfferRepository
{
	Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default);
	Task<TradeOffer?> Get(Guid offerId);
	IEnumerable<TradeOffer> GetUserBuyOffers(Guid userId);
	IEnumerable<TradeOffer> GetUserSellOffers(Guid userId);
	IEnumerable<TradeOffer> FindBuyOffers(Guid stockId, decimal price);
	IEnumerable<TradeOffer> FindSellOffers(Guid stockId, decimal price);
	Task Add(TradeOffer offer, CancellationToken cancellationToken = default);
	Task<bool> DecreaseOfferAmount(Guid offerId, int amount);
	bool Cancel(Guid offerId);
}
