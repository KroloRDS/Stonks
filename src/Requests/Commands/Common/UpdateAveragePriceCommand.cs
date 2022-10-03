using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Models;
using Stonks.Requests.Queries.Common;

namespace Stonks.Requests.Commands.Common;

public record UpdateAveragePriceCommand(Guid StockId) : IRequest;

public class UpdateAveragePriceCommandHandler :
	IRequestHandler<UpdateAveragePriceCommand>
{
	public const decimal DEFAULT_PRICE = 1M;
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;

	public UpdateAveragePriceCommandHandler(AppDbContext ctx,
		IMediator mediator)
	{
		_ctx = ctx;
		_mediator = mediator;
	}

	public async Task<Unit> Handle(UpdateAveragePriceCommand request,
		CancellationToken cancellationToken)
	{
		var stockId = request.StockId;
		if (_ctx.GetById<Stock>(stockId).Bankrupt) return Unit.Value;

		var currentPrice = await _ctx.AvgPriceCurrent
			.SingleOrDefaultAsync(x => x.StockId == stockId, cancellationToken);
		var transactions = await GetTransactions(
			stockId, currentPrice?.Created, cancellationToken);
		var newPrice = AverageFromTransactions(
			transactions, currentPrice);

		if (currentPrice is null)
			AddFirstAvgPrice(newPrice, stockId);
		else
			UpdateExistingPrice(newPrice, currentPrice);

		await _ctx.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}

	private void UpdateExistingPrice(StockPriceModel model,
		AvgPriceCurrent currentPrice)
	{
		_ctx.Add(new AvgPrice
		{
			StockId = currentPrice.StockId,
			DateTime = currentPrice.Created,
			SharesTraded = currentPrice.SharesTraded,
			Price = currentPrice.Price,
			PriceNormalised = currentPrice.Price
		});

		currentPrice.Created = DateTime.Now;
		currentPrice.SharesTraded = model.SharesTraded;
		currentPrice.Price = model.Amount;
	}

	private void AddFirstAvgPrice(StockPriceModel model, Guid stockId)
	{
		_ctx.Add(new AvgPriceCurrent
		{
			StockId = stockId,
			Created = DateTime.Now,
			SharesTraded = model.SharesTraded,
			Price = model.Amount
		});
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