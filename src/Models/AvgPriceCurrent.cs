using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class AvgPriceCurrent : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }
	[Required]
	public DateTime DateTime { get; set; }

	public ulong TotalAmountTraded { get; set; }

	[Column(TypeName = "decimal(8,2)")]
	public decimal Amount { get; set; }
}
