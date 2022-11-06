using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace Stonks.CQRS.Commands;

public record UpdateAveragePriceCommand(Guid StockId) : IRequest;

public class UpdateAveragePriceCommandHandler :
    BaseCommand<UpdateAveragePriceCommand>
{
    public const decimal DEFAULT_PRICE = 1M;

    public UpdateAveragePriceCommandHandler(AppDbContext ctx,
        IMediator mediator) : base(ctx, mediator) {}

    public override async Task<Unit> Handle(UpdateAveragePriceCommand request,
        CancellationToken cancellationToken)
    {
		var stockId = request.StockId;
		var stock = await _ctx.GetById<Stock>(stockId);
		if (stock.Bankrupt) return Unit.Value;

		var currentPrice = await _ctx.AvgPriceCurrent
			.SingleOrDefaultAsync(x => x.StockId == stockId, cancellationToken);
		var transactions = await GetTransactions(
			stockId, currentPrice?.Created, cancellationToken);
		var newPrice = AverageFromTransactions(
			transactions, currentPrice);

		if (currentPrice is null)
			await AddFirstAvgPrice(newPrice, stockId, cancellationToken);
		else
			await UpdateExistingPrice(newPrice, currentPrice, cancellationToken);
		return Unit.Value;
    }

	private async Task UpdateExistingPrice(StockPriceModel model,
        AvgPriceCurrent currentPrice, CancellationToken cancellationToken)
    {
        await _ctx.AddAsync(new AvgPrice
        {
            StockId = currentPrice.StockId,
            DateTime = currentPrice.Created,
            SharesTraded = currentPrice.SharesTraded,
            Price = currentPrice.Price,
            PriceNormalised = currentPrice.Price
        }, cancellationToken);

        currentPrice.Created = DateTime.Now;
        currentPrice.SharesTraded = model.SharesTraded;
        currentPrice.Price = model.Amount;
    }

    private async Task AddFirstAvgPrice(StockPriceModel model, Guid stockId,
		CancellationToken cancellationToken)
    {
        await _ctx.AddAsync(new AvgPriceCurrent
        {
            StockId = stockId,
            Created = DateTime.Now,
            SharesTraded = model.SharesTraded,
            Price = model.Amount
        }, cancellationToken);
    }

    private async Task<IEnumerable<Transaction>> GetTransactions(Guid stockId,
        DateTime? from, CancellationToken cancellationToken)
    {
        var query = new GetTransactionsQuery(stockId, from);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Transactions;
    }

    private static StockPriceModel AverageFromTransactions(
        IEnumerable<Transaction> transactions, AvgPriceCurrent? currentPrice)
    {
        var sharesTraded = currentPrice?.SharesTraded ?? 0UL;
        var priceSum = (currentPrice?.Price ?? 0M) * sharesTraded;
        var ifNoTrade = currentPrice is null ? DEFAULT_PRICE : 0M;

        foreach (var transaction in transactions)
        {
            priceSum += transaction.Amount * transaction.Price;
            sharesTraded += (ulong)transaction.Amount;
        }

        var avgPrice = sharesTraded > 0 ?
            priceSum / sharesTraded : ifNoTrade;

        return new StockPriceModel(avgPrice, sharesTraded);
    }

    private record StockPriceModel(decimal Amount, ulong SharesTraded);
}