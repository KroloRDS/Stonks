using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;
using Stonks.Requests.Commands.Trade;

namespace Stonks.Managers.Trade;

public class TransferSharesManager : ITransferSharesManager
{
	private readonly AppDbContext _ctx;

	public TransferSharesManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void TransferShares(TransferSharesCommand? command)
	{
		(var stockId, var userId, var amount) = ValidateCommand(command);

		if (command!.BuyFromUser is not true)
		{
			var stock = _ctx.GetById<Stock>(stockId);
			if (stock.PublicallyOfferredAmount < amount)
				throw new NoPublicStocksException();

			stock.PublicallyOfferredAmount -= amount;
			AddTransactionLog(stockId, userId, null, amount);
		}
		else
		{
			var sellerId = _ctx.EnsureUserExist(command.SellerId);
			TakeStocksFromUser(stockId, sellerId, amount);
			AddTransactionLog(stockId, userId, sellerId, amount);
		}

		GiveStocksToUser(stockId, userId, amount);
	}

	private (Guid, Guid, int) ValidateCommand(TransferSharesCommand? command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.BuyFromUser is not true && command.SellerId is not null)
			throw new ExtraRefToSellerException();

		var stock = _ctx.GetById<Stock>(command.StockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		return (stock.Id, _ctx.EnsureUserExist(command.BuyerId),
			command.Amount.AssertPositive());
	}

	private void GiveStocksToUser(Guid stockId, Guid userId, int amount)
	{
		var ownership = _ctx.GetShares(userId, stockId);

		if (ownership is null)
		{
			_ctx.Add(new Share
			{
				Amount = amount,
				OwnerId = userId.ToString(),
				StockId = stockId
			});
		}
		else
		{
			ownership.Amount += amount;
		}
	}

	private void TakeStocksFromUser(Guid stockId, Guid userId, int amount)
	{
		var ownership = _ctx.GetShares(userId, stockId);

		if (ownership is null || ownership.Amount < amount)
			throw new NoStocksOnSellerException();

		ownership.Amount -= amount;
	}

	private void AddTransactionLog(Guid stockId, Guid buyerId, Guid? sellerId, int amount)
	{
		_ctx.Add(new Transaction
		{
			StockId = stockId,
			BuyerId = buyerId.ToString(),
			SellerId = sellerId.ToString(),
			Amount = amount,
			Timestamp = DateTime.Now
		});
	}
}
