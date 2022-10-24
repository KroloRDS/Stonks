using Stonks.Models;
using Stonks.ViewModels;

namespace Stonks.Responses.Common;

public record GetStockViewModelResponse(Stock Stock,
    IEnumerable<AvgPrice> Prices, decimal CurrentPrice) : BaseViewModel;
