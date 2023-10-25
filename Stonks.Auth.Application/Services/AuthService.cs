using Stonks.Auth.Db;
using Stonks.Auth.Domain.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Stonks.Auth.Application.Services;

public interface IAuthService
{
	Task<Guid?> GetUserIdFromToken(Guid token,
		CancellationToken cancellationToken = default);

	Task<Guid> RefreshToken(Guid userId,
		CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
	private readonly IDbWriter _dbWriter;
	private readonly IUserRepository _user;

	public AuthService(IDbWriter dbWriter, IUserRepository user)
	{
		_dbWriter = dbWriter;
		_user = user;
	}

	private static readonly TimeSpan TokenExpiration =
		TimeSpan.FromMinutes(10);

	public static DateTime GetTokenExpiration() =>
		DateTime.Now.Add(TokenExpiration);

	public static byte[] Hash(string text, short salt)
	{
		var salted = text + salt.ToString();
		var bytes = Encoding.UTF8.GetBytes(salted);
		var hash = SHA256.HashData(bytes);
		return hash;
	}

	public async Task<Guid?> GetUserIdFromToken(Guid token,
		CancellationToken cancellationToken = default) =>
		await _user.GetUserIdFromToken(token, cancellationToken);

	public async Task<Guid> RefreshToken(Guid userId,
		CancellationToken cancellationToken = default)
	{
		var token = await _user.RefreshToken(userId,
			GetTokenExpiration(), cancellationToken);
		await _dbWriter.SaveChanges(cancellationToken);
		return token;
	}
}
