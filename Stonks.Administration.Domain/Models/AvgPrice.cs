namespace Stonks.Administration.Domain.Models;

public class AvgPrice
{
	public Guid StockId { get; set; }
	public DateTime DateTime { get; set; }

	public ulong SharesTraded { get; set; }

	public decimal Price { get; set; }
}
