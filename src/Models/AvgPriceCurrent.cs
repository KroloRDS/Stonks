using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class AvgPriceCurrent : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	public ulong SharesTraded { get; set; }
	[Required]
	public DateTime Created { get; set; }

	[Column(TypeName = "decimal(8,2)")]
	public decimal Price { get; set; }
}
