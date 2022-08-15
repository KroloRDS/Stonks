using Stonks.Models;

namespace Stonks.ViewModels;

public class StockViewModel
{
	public Stock Stock { get; set; }
	public AvgPriceCurrent CurrentPrice { get; set; }
	public IEnumerable<AvgPrice> Prices { get; set; }

	public StockViewModel(Stock stock, IEnumerable<AvgPrice> prices,
		AvgPriceCurrent currentPrice)
	{
		Stock = stock ?? throw new ArgumentNullException(nameof(stock));
		CurrentPrice = currentPrice ?? throw new ArgumentNullException(nameof(currentPrice));
		Prices = prices ?? throw new ArgumentNullException(nameof(prices));
	}
}
