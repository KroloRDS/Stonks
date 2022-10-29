using System.ComponentModel.DataAnnotations;

namespace Stonks.Models;

public class Share
{
	[Required]
	public Guid OwnerId { get; set; }
	[Required]
	public User Owner { get; set; }

	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	[ConcurrencyCheck]
	public int Amount { get; set; }
}
