using Stonks.Auth.Domain.Models;

namespace Stonks.Auth.Domain.Repositories;

public interface IUserRepository
{
	Task<bool> LoginExist(string login,
		CancellationToken cancellationToken = default);

	Task<User> GetUserFromLogin(string login,
		CancellationToken cancellationToken = default);

	Task<bool> Add(User user,
		CancellationToken cancellationToken = default);
}
