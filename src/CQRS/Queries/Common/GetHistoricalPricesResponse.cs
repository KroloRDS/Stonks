using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetHistoricalPricesResponse(IEnumerable<AvgPrice> Prices);
