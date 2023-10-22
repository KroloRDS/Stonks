using System.ComponentModel.DataAnnotations;

namespace Stonks.Common.Db.EntityFrameworkModels;

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

	public int Amount { get; set; }
}
