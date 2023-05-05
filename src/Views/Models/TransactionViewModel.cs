using Stonks.Data.Models;

namespace Stonks.Views.Models;

public class TransactionViewModel
{
	public string Type { get; set; }
	public int Amount { get; set; }
	public decimal Price { get; set; }

	public TransactionViewModel(Transaction transaction, Guid userId)
	{
		Amount = transaction.Amount;
		Price = transaction.Price;
		Type = transaction.BuyerId == userId ?
			"BUY" : "SELL";
	}
}
