using Stonks.Models;

namespace Stonks.Contracts.Queries.Common;

public record GetHistoricalPricesResponse(IEnumerable<AvgPrice> Prices);
