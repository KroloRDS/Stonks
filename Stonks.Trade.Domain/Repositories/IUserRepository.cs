namespace Stonks.Trade.Domain.Repositories;

public interface IUserRepository
{
	Task<decimal> GetBalance(Guid userId,
		CancellationToken cancellationToken = default);

	Task ChangeBalance(Guid userId, decimal balance,
		CancellationToken cancellationToken = default);
}
