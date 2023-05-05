using Stonks.Data.Models;

namespace Stonks.Views.Models;

public class StockViewModel
{
	public Guid Id { get; set; }
	public decimal Profit { get; set; }
	public decimal Price { get; set; }
	public int OwnedAmount { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
	public IEnumerable<AvgPrice> Prices { get; set; } =
		Enumerable.Empty<AvgPrice>();
	public IEnumerable<TradeOffer> Offers { get; set; } =
		Enumerable.Empty<TradeOffer>();
	public IEnumerable<Transaction> Transactions { get; set; } =
		Enumerable.Empty<Transaction>();
}
