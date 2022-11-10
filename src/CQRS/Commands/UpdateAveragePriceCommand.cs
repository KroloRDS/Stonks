using MediatR;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;

namespace Stonks.CQRS.Commands;

public record UpdateAveragePriceCommand(Guid StockId) : IRequest;

public class UpdateAveragePriceCommandHandler :
    BaseCommand<UpdateAveragePriceCommand>
{
    public UpdateAveragePriceCommandHandler(AppDbContext ctx,
        IMediator mediator) : base(ctx, mediator) {}

    public override async Task<Unit> Handle(UpdateAveragePriceCommand request,
        CancellationToken cancellationToken)
    {
		var stock = await _ctx.GetById<Stock>(request.StockId);
		if (stock.Bankrupt) return Unit.Value;

		var transactions = await GetTransactions(stock.Id,
			stock.LastPriceUpdate, cancellationToken);
		var newPrice = AverageFromTransactions(transactions, stock);

		await UpdatePrice(newPrice, stock, cancellationToken);
		return Unit.Value;
	}

	private async Task<IEnumerable<Transaction>> GetTransactions(Guid stockId,
		DateTime? from, CancellationToken cancellationToken)
	{
		var query = new GetTransactionsQuery
		{
			StockId = stockId,
			FromDate = from
		};
		var result = await _mediator.Send(query, cancellationToken);
		return result.Transactions;
	}

	private static StockPriceModel AverageFromTransactions(
		IEnumerable<Transaction> transactions, Stock stock)
	{
		var sharesTraded = stock.SharesTraded;
		var priceSum = stock.Price * sharesTraded;

		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			sharesTraded += (ulong)transaction.Amount;
		}

		var avgPrice = sharesTraded > 0 ?
			priceSum / sharesTraded : Stock.DEFAULT_PRICE;

		return new StockPriceModel(avgPrice, sharesTraded);
	}

	private async Task UpdatePrice(StockPriceModel model,
		Stock stock, CancellationToken cancellationToken)
    {
		var updated = DateTime.Now;
		stock.Price = model.Price;
		stock.LastPriceUpdate = updated;
		stock.SharesTraded = model.SharesTraded;

		await _ctx.AddAsync(new AvgPrice
        {
            StockId = stock.Id,
            DateTime = stock.LastPriceUpdate!.Value,
            SharesTraded = model.SharesTraded,
            Price = model.Price
        }, cancellationToken);
    }

	private record StockPriceModel(decimal Price, ulong SharesTraded);
}