using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class HistoricalPrice
{
	public Guid Id { get; set; }
	public DateTime DateTime { get; set; }
	public Stock Stock { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal Price { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal PriceNormalised { get; set; }
}
