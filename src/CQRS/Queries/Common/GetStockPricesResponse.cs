using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetStockPricesResponse(IEnumerable<AvgPrice> Prices);
