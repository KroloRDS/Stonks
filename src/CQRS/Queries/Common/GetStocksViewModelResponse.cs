using Stonks.Views.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetStocksViewModelResponse(IEnumerable<StockViewModel> Stocks);
