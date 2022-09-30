using MediatR;
using Stonks.Data;
using Stonks.Helpers;
using Stonks.Models;

namespace Stonks.Requests.Commands.Trade;

public record TransferSharesCommand(
	Guid StockId,
	int Amount,
	Guid BuyerId,
	bool BuyFromUser,
	Guid? SellerId = null) : IRequest;

public class TransferSharesCommandHandler :
	IRequestHandler<TransferSharesCommand>
{
	private readonly AppDbContext _ctx;

	public TransferSharesCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(TransferSharesCommand request,
		CancellationToken cancellationToken)
	{
		var order = await ValidateCommand(request, cancellationToken);

		var buyTask = request.BuyFromUser ?
			BuyFromUser(order, request.SellerId, cancellationToken) :
			BuyFromCompany(order, cancellationToken);

		var giveTask = GiveSharesToUser(order, cancellationToken);

		await Task.WhenAll(buyTask, giveTask);
		return Unit.Value;
	}

	private async Task<TransferOrder> ValidateCommand(
		TransferSharesCommand command, CancellationToken cancellationToken)
	{
		if (command.BuyFromUser is not true && command.SellerId is not null)
			throw new ExtraRefToSellerException();

		var userId = _ctx.EnsureUserExistAsync(
			command.BuyerId, cancellationToken);

		var stock = await _ctx.GetByIdAsync<Stock>(command.StockId);

		if (stock.Bankrupt)
			throw new BankruptStockException();

		return new TransferOrder(stock.Id,
			await userId, command.Amount.AssertPositive());
	}

	private async Task BuyFromCompany(TransferOrder order,
		CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetByIdAsync<Stock>(order.StockId);

		if (stock.PublicallyOfferredAmount < order.Amount)
			throw new NoPublicStocksException();

		stock.PublicallyOfferredAmount -= order.Amount;

		await AddTransactionLog(order, null, cancellationToken);
	}

	private async Task BuyFromUser(TransferOrder order,
		Guid? sellerId, CancellationToken cancellationToken)
	{
		var sellerValidatedId = await _ctx.EnsureUserExistAsync(
			sellerId, cancellationToken);

		await Task.WhenAll(
			TakeSharesFromUser(order, sellerValidatedId, cancellationToken),
			AddTransactionLog(order, sellerValidatedId, cancellationToken));
	}

	private async Task GiveSharesToUser(TransferOrder order,
		CancellationToken cancellationToken)
	{
		var ownership = await _ctx.GetSharesAsync(order.UserId,
			order.StockId, cancellationToken);

		if (ownership is null)
		{
			await _ctx.AddAsync(new Share
			{
				Amount = order.Amount,
				OwnerId = order.UserId.ToString(),
				StockId = order.StockId
			}, cancellationToken);
		}
		else
		{
			ownership.Amount += order.Amount;
		}
	}

	private async Task TakeSharesFromUser(TransferOrder order,
		Guid sellerId, CancellationToken cancellationToken)
	{
		var ownership = await _ctx.GetSharesAsync(sellerId,
			order.StockId, cancellationToken);

		if (ownership is null || ownership.Amount < order.Amount)
			throw new NoStocksOnSellerException();

		ownership.Amount -= order.Amount;
	}

	private async Task AddTransactionLog(TransferOrder order,
		Guid? sellerId, CancellationToken cancellationToken)
	{
		await _ctx.AddAsync(new Transaction
		{
			StockId = order.StockId,
			BuyerId = order.UserId.ToString(),
			SellerId = sellerId.ToString(),
			Amount = order.Amount,
			Timestamp = DateTime.Now
		}, cancellationToken);
	}

	private record TransferOrder(Guid StockId, Guid UserId, int Amount);
}
