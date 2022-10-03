using Stonks.Models;

namespace Stonks.ViewModels;

public record StockViewModel(Stock Stock,
	IEnumerable<AvgPrice> Prices, decimal CurrentPrice);
