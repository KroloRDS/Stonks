using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Stonks.Models;

public class StockOwnership : HasId
{
	public IdentityUser Owner { get; set; }
	public Stock Stock { get; set; }

	[ConcurrencyCheck]
	public int Amount { get; set; }
}
