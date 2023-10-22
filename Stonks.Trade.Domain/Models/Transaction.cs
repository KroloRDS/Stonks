namespace Stonks.Trade.Domain.Models;

public class Transaction
{
	public Guid StockId { get; set; }
	public Guid BuyerId { get; set; }
	public Guid? SellerId { get; set; }
	public int Amount { get; set; }
	public decimal Price { get; set; }
	public DateTime Timestamp { get; set; }
}
