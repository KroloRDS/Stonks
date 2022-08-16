using Stonks.Models;

namespace Stonks.Responses.Common;

public record GetHistoricalPricesResponse(IEnumerable<AvgPrice> Prices);
