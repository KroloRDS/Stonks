namespace Stonks.Common.Utils.Configuration;

public class JwtConfiguration
{
	public string Issuer { get; init; } = string.Empty;
	public string Audience { get; init; } = string.Empty;
	public string SigningKey { get; set; } = string.Empty;
	public int ExpirationMinutes { get; init; }
}
