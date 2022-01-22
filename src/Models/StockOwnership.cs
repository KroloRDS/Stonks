using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Stonks.Models;

public class StockOwnership : HasId
{
	[Required]
	public string OwnerId { get; set; }
	[Required]
	public User Owner { get; set; }

	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	[ConcurrencyCheck]
	public int Amount { get; set; }
}
