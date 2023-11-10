namespace Stonks.Administration.Domain.Repositories;

public interface IShareRepository
{
	Task<int> TotalAmountOfShares(Guid stockId,
		CancellationToken cancellationToken = default);
	void RemoveShares(Guid stockId);
}
