using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;

public class StockOwnership
{
	public Guid Id { get; set; }
	public IdentityUser Owner { get; set; }
	public Stock Stock { get; set; }
	public int Amount { get; set; }
}
