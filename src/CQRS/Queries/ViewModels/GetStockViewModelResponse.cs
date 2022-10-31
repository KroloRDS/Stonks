using Stonks.Data.Models;

namespace Stonks.CQRS.Queries.ViewModels;

public record GetStockViewModelResponse(Stock Stock,
    IEnumerable<AvgPrice> Prices, decimal CurrentPrice) : BaseViewModel;
