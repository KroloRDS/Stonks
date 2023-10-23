namespace Stonks.Administration.Domain.Repositories;

public interface IOfferRepository
{
	Task<int> PublicallyOfferdAmount(Guid stockId,
		CancellationToken cancellationToken = default);
	Task AddNewPublicOffers(int amount,
		CancellationToken cancellationToken = default);
	void SetExistingPublicOffersAmount(int amount);
	void RemoveOffers(Guid stockId);
}
