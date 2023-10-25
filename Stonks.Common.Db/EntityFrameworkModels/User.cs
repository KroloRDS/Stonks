using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Common.Db.EntityFrameworkModels;
public class User : HasId
{
	[Column(TypeName = "decimal(11,2)")]
	public decimal Funds { get; set; }

	[Required]
	public string Login { get; set; } = string.Empty;

	[Required]
	[MaxLength(32)]
	public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

	[Required]
	public short Salt { get; set; }

	public IEnumerable<Role> Roles { get; set; } = Enumerable.Empty<Role>();
}

public enum Role
{
	Trader,
	Admin
}
