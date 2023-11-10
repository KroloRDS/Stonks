using Microsoft.IdentityModel.Tokens;
using Stonks.Common.Utils.Models.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Stonks.Common.Utils.Services;

public interface IAuthService
{
	(Guid?, IEnumerable<string>) ReadToken(string? token);

	string CreateAccessToken(Guid userId, string login,
		IEnumerable<string> roles);
}

public class AuthService : IAuthService
{
	private readonly JwtConfiguration _jwtConfiguration;
	private readonly IStonksLogger _logger;

	public AuthService(JwtConfiguration jwtConfiguration,
		ILogProvider logProvider)
	{
		_jwtConfiguration = jwtConfiguration;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public static byte[] Hash(string text, short salt)
	{
		var salted = text + salt.ToString();
		var bytes = Encoding.UTF8.GetBytes(salted);
		var hash = SHA256.HashData(bytes);
		return hash;
	}

	public (Guid?, IEnumerable<string>) ReadToken(string? token)
	{
		if (token is null)
			return (null, Enumerable.Empty<string>());

		var claims = GetTokenClaims(token);
		var nameId = claims.FirstOrDefault(x => x.Type.EndsWith("nameidentifier"))?.Value;
		Guid? userId = nameId is null ? null : Guid.Parse(nameId);
		var roles = claims.Where(x => x.Type.EndsWith("role"))
			.Select(x => x.Value);

		return (userId, roles);
	}

	private IEnumerable<Claim> GetTokenClaims(string token)
	{
		var keyBytes = Encoding.ASCII.GetBytes(_jwtConfiguration.SigningKey);
		var validations = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
			ValidIssuer = _jwtConfiguration.Issuer,
			ValidateIssuer = true,
			ValidAudience = _jwtConfiguration.Audience,
			ValidateAudience = true
		};

		try
		{
			var result = new JwtSecurityTokenHandler()
				.ValidateToken(token, validations, out _);
			return result?.Claims ?? Enumerable.Empty<Claim>();
		}
		catch (Exception ex)
		{
			_logger.Log("JWT decode error", ex, token);
			return Enumerable.Empty<Claim>();
		}
	}

	public string CreateAccessToken(Guid userId,
		string login, IEnumerable<string> roles)
	{
		var claims = GetClaims(userId, login, roles);
		var credentials = GetSigningCredentials();

		var token = new JwtSecurityToken(
			issuer: _jwtConfiguration.Issuer,
			audience: _jwtConfiguration.Audience,
			claims: claims,
			expires: GetExpirationTime(),
			signingCredentials: credentials);

		var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
		return rawToken;
	}

	private SigningCredentials GetSigningCredentials()
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
		DateTime.Now.AddMinutes(_jwtConfiguration.ExpirationMinutes);
}
