using Microsoft.IdentityModel.Tokens;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Stonks.Auth.Application.Services;

public interface IAuthService
{
	string CreateAccessToken(Guid userId, string login,
		IEnumerable<string> roles);
}

public class AuthService : IAuthService
{
	private readonly JwtConfiguration _jwtConfiguration;
	private readonly ICurrentTime _currentTime;

	public AuthService(JwtConfiguration jwtConfiguration,
		ICurrentTime currentTime)
	{
		_jwtConfiguration = jwtConfiguration;
		_currentTime = currentTime;
	}

	public static byte[] Hash(string text, short salt)
	{
		var salted = text + salt.ToString();
		var bytes = Encoding.UTF8.GetBytes(salted);
		var hash = SHA256.HashData(bytes);
		return hash;
	}

	public static (Guid?, IEnumerable<string>) ReadToken(string? token)
	{
		if (token is null)
			return (null, Enumerable.Empty<string>());

		var key = Environment.GetEnvironmentVariable(Consts.JWT_SIGNING_KEY) ??
			throw new Exception($"Missing {Consts.JWT_SIGNING_KEY} env variable");

		var keyBytes = Encoding.ASCII.GetBytes(key);
		var handler = new JwtSecurityTokenHandler();
		var validations = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
			ValidateIssuer = true,
			ValidateAudience = true
		};

		var decrypted = handler.ValidateToken(token, validations, out _);
		var sub = decrypted.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
		Guid? userId = sub is null ? null : Guid.Parse(sub);
		var roles = decrypted.Claims.Where(x => x.Type == "role")
			.Select(x => x.Value);

		return (userId, roles);
	}

	public string CreateAccessToken(Guid userId,
		string login, IEnumerable<string> roles)
	{
		var claims = GetClaims(userId, login, roles);
		var credentials = GetSigningCredentials(_jwtConfiguration.SigningKey);

		var token = new JwtSecurityToken(
			issuer: _jwtConfiguration.Issuer,
			audience: _jwtConfiguration.Audience,
			claims: claims,
			expires: GetExpirationTime(),
			signingCredentials: credentials);

		var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
		return rawToken;
	}

	private SigningCredentials GetSigningCredentials(string key)
	{
		var bytes = Encoding.UTF8.GetBytes(_jwtConfiguration.SigningKey);
		var symmetricKey = new SymmetricSecurityKey(bytes);

		return new SigningCredentials(
			symmetricKey,
			SecurityAlgorithms.HmacSha256);
	}

	private IEnumerable<Claim> GetClaims(Guid userId,
		string login, IEnumerable<string> roles)
	{
		var claims = new List<Claim>()
		{
			new Claim("sub", userId.ToString()),
			new Claim("name", login),
			new Claim("aud", _jwtConfiguration.Audience)
		};

		var roleClaims = roles.Select(x => new Claim("role", x));
		claims.AddRange(roleClaims);

		return claims;
	}

	private DateTime GetExpirationTime() =>
		_currentTime.Get().AddMinutes(_jwtConfiguration.ExpirationMinutes);
}
