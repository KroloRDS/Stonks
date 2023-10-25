using Stonks.Auth.Domain.Models;

namespace Stonks.Auth.Domain.Repositories;

public interface IUserRepository
{
	Task<bool> LoginExist(string login,
		CancellationToken cancellationToken = default);

	Task<User> GetUserFromLogin(string login,
		CancellationToken cancellationToken = default);

	Task<Guid?> GetUserIdFromToken(Guid token,
		CancellationToken cancellationToken = default);

	Task<Guid> RefreshToken(Guid userId, DateTime tokenExpiration,
		CancellationToken cancellationToken = default);

	Task<Guid> Add(string login, short salt, byte[] hash,
		CancellationToken cancellationToken = default);
}
