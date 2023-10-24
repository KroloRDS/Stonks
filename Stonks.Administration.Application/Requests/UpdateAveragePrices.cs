using MediatR;
using Stonks.Administration.Db;
using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Administration.Application.Requests;

public record UpdateAveragePrices : IRequest<Response>;

public class UpdateAveragePricesHandler
	: IRequestHandler<UpdateAveragePrices, Response>
{
	private readonly IDbWriter _writer;
	private readonly ICurrentTime _currentTime;
	private readonly IStockRepository _stock;
	private readonly IPriceRepository _price;
	private readonly ITransactionRepository _transaction;
	private readonly IStonksLogger _logger;

	public UpdateAveragePricesHandler(IDbWriter writer,
		ICurrentTime currentTime,
		IStockRepository stock,
		IPriceRepository price,
		ITransactionRepository transaction,
		ILogProvider logProvider)
	{
		_stock = stock;
		_price = price;
		_writer = writer;
		_currentTime = currentTime;
		_transaction = transaction;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response> Handle(UpdateAveragePrices request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var stocks = await _stock.GetActive(cancellationToken);
			var transaction = _writer.BeginTransaction();
			var updates = stocks.Select(x => UpdateSingle(
				x.Id, cancellationToken));
			await Task.WhenAll(updates);
			await _writer.CommitTransaction(transaction, cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task UpdateSingle(Guid stockId,
		CancellationToken cancellationToken = default)
	{
		var currentPrice = await _price.Current(stockId);
		var transactions = _transaction.Get(stockId);
		var (price, sharesTraded) = AverageFromTransactions(
			transactions, currentPrice);

		await _price.Add(new AvgPrice
		{
			StockId = stockId,
			DateTime = _currentTime.Get(),
			SharesTraded = sharesTraded,
			Price = price
		}, cancellationToken);
	}

	private static (decimal, ulong) AverageFromTransactions(
		IEnumerable<Transaction> transactions, AvgPrice currentPrice)
	{
		var sharesTraded = currentPrice.SharesTraded;
		var priceSum = currentPrice.Price * sharesTraded;

		foreach (var transaction in transactions)
		{
			priceSum += transaction.Amount * transaction.Price;
			sharesTraded += (ulong)transaction.Amount;
		}

		var avgPrice = sharesTraded > 0 ?
			priceSum / sharesTraded : Stock.DEFAULT_PRICE;

		return (avgPrice, sharesTraded);
	}
}
