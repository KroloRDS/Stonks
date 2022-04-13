namespace Stonks.DTOs;

public class TransferSharesCommand
{
	public Guid? StockId { get; set; }
	public Guid? BuyerId { get; set; }
	public Guid? SellerId { get; set; }
	public int? Amount { get; set; }
	public bool? BuyFromUser { get; set; }
}
