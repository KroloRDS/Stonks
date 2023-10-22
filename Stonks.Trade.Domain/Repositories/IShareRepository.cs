namespace Stonks.Trade.Domain.Repositories;

public interface IShareRepository
{
	Task<int> GetOwnedAmount(Guid stockId, Guid userId,
		CancellationToken cancellationToken);

	Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default);

	Task GiveSharesToUser(Guid stockId, Guid userId,
		int amount, CancellationToken cancellationToken);

	Task TakeSharesFromUser(Guid stockId, Guid userId,
		int amount, CancellationToken cancellationToken);
}
