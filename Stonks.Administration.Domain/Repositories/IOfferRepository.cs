namespace Stonks.Administration.Domain.Repositories;

public interface IOfferRepository
{
	Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default);
	Task AddPublicOffers(int amount,
		CancellationToken cancellationToken = default);
	void RemoveOffers(Guid stockId);
}
