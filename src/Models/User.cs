using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;
public class User : IdentityUser
{
	public string? PayPalEmail { get; set; }
}
