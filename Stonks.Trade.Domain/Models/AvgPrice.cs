namespace Stonks.Trade.Domain.Models;

public class AvgPrice
{
	public Guid StockId { get; set; }
	public decimal Price { get; set; }
}
