using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Helpers;

public record TransferSharesCommand
{
	public Guid StockId { get; set; }

	public int Amount { get; set; }
	public Guid BuyerId { get; set; }
	public bool BuyFromUser { get; set; }
	public Guid? SellerId { get; set; } = null;
}

public interface ITransferShares
{
	Task Handle(TransferSharesCommand command,
		CancellationToken cancellationToken);
}

public class TransferShares : ITransferShares
{
    private readonly AppDbContext _ctx;

    public TransferShares(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Handle(TransferSharesCommand command,
        CancellationToken cancellationToken)
    {
        await ValidateCommand(command, cancellationToken);

        if (command.BuyFromUser)
			await TakeSharesFromUser(command, cancellationToken);
		else
			await TakeSharesFromCompany(command);

        await GiveSharesToUser(command, cancellationToken);
		await AddTransactionLog(command, cancellationToken);
	}

    private async Task ValidateCommand(TransferSharesCommand command,
		CancellationToken cancellationToken)
    {
		command.Amount.AssertPositive();
        if (command.BuyFromUser is not true && command.SellerId is not null)
            throw new ExtraRefToSellerException();

        await _ctx.EnsureExist<User>(command.BuyerId, cancellationToken);

        var stock = await _ctx.GetById<Stock>(command.StockId);
        if (stock.Bankrupt) throw new BankruptStockException();
    }

	private async Task TakeSharesFromUser(TransferSharesCommand command,
		CancellationToken cancellationToken)
	{
		await _ctx.EnsureExist<User>(command.SellerId, cancellationToken);
		var ownership = await _ctx.GetShares(command.SellerId,
			command.StockId, cancellationToken);

		if (ownership is null || ownership.Amount < command.Amount)
			throw new NoStocksOnSellerException();

		ownership.Amount -= command.Amount;
	}

	private async Task TakeSharesFromCompany(TransferSharesCommand command)
    {
        var stock = await _ctx.GetById<Stock>(command.StockId);

        if (stock.PublicallyOfferredAmount < command.Amount)
            throw new NoPublicStocksException();

        stock.PublicallyOfferredAmount -= command.Amount;
    }

	private async Task GiveSharesToUser(TransferSharesCommand command,
        CancellationToken cancellationToken)
    {
        var ownership = await _ctx.GetShares(command.BuyerId,
			command.StockId, cancellationToken);

		if (ownership != null)
		{
			ownership.Amount += command.Amount;
			return;
		}

		await _ctx.AddAsync(new Share
		{
			Amount = command.Amount,
			OwnerId = command.BuyerId,
			StockId = command.StockId
		}, cancellationToken);
	}

    private async Task AddTransactionLog(TransferSharesCommand command,
		CancellationToken cancellationToken)
    {
        await _ctx.AddAsync(new Transaction
        {
            StockId = command.StockId,
            BuyerId = command.BuyerId,
            SellerId = command.SellerId,
            Amount = command.Amount,
            Timestamp = DateTime.Now
        }, cancellationToken);
    }
}
