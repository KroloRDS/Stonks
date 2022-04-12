using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;
public class User : IdentityUser
{
	public string? PayPalEmail { get; set; }

	[Column(TypeName = "decimal(11,2)")]
	public decimal Funds { get; set; }
}
