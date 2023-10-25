namespace Stonks.Auth.Domain.Models;

public class User
{
	public Guid Id { get; set; }
	public string Login { get; set; } = string.Empty;
	public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
	public short Salt { get; set; }
	public IEnumerable<Role> Roles { get; set; } = Enumerable.Empty<Role>();
}

public enum Role
{
	Trader,
	Admin
}
