using Stonks.Data.Models;

namespace Stonks.Views.Models;

public class StockViewModel
{
	public decimal Profit { get; set; }
	public decimal CurrentPrice { get; set; }
	public int OwnedAmount { get; set; }
	public string StockName { get; set; } = string.Empty;
	public string StockSymbol { get; set; } = string.Empty;
	public IEnumerable<AvgPrice> Prices { get; set; } =
		Enumerable.Empty<AvgPrice>();
	public IEnumerable<TradeOffer> Offers { get; set; } =
		Enumerable.Empty<TradeOffer>();
	public IEnumerable<Transaction> Transactions { get; set; } =
		Enumerable.Empty<Transaction>();
}
