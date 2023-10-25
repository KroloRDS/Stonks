using Stonks.Trade.Domain.Models;

namespace Stonks.Trade.Domain.Repositories;

public interface IOfferRepository
{
	Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default);
	Task<TradeOffer?> Get(Guid offerId,
		CancellationToken cancellationToken = default);
	Task<IEnumerable<TradeOffer>> GetUserBuyOffers(Guid userId,
		CancellationToken cancellationToken = default);
	Task<IEnumerable<TradeOffer>> GetUserSellOffers(Guid userId,
		CancellationToken cancellationToken = default);
	Task<IEnumerable<TradeOffer>> FindBuyOffers(Guid stockId, decimal price,
		CancellationToken cancellationToken = default);
	Task<IEnumerable<TradeOffer>> FindSellOffers(Guid stockId, decimal price,
		CancellationToken cancellationToken = default);

	Task Add(TradeOffer offer, CancellationToken cancellationToken = default);
	Task<bool> DecreaseOfferAmount(Guid offerId, int amount);
	bool Cancel(Guid offerId);
}
