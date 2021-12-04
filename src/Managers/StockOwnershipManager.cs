using Stonks.Data;
using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers;

public class StockOwnershipManager : IStockOwnershipManager
{
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public StockOwnershipManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void BuyStock(BuyStockCommand? command)
	{
		(var stockId, var userId, var amount) = ValidateCommand(command);

		if (command?.BuyFromUser != true)
		{
			var stock = _ctx.GetById<Stock>(stockId);
			if (stock.PublicallyOfferredAmount < amount)
				throw new InvalidOperationException("Not enough publically offered stocks");

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

		_ctx.SaveChanges();
	}

	private (Guid, string, int) ValidateCommand(BuyStockCommand? command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.BuyFromUser != true && command.SellerId is not null)
			throw new ArgumentException("Reference to seller is not necessary when not buying from user", nameof(command));

		return (_ctx.EnsureExist<Stock>(command.StockId),
			_ctx.EnsureUserExist(command.BuyerId),
			command.Amount.ToPositive());
	}

	private void GiveStocksToUser(Guid stockId, string userId, int amount)
	{
		var ownership = _ctx.GetStockOwnership(userId, stockId);

		if (ownership is null)
		{
			_ctx.Add(new StockOwnership
			{
				Amount = amount,
				OwnerId = userId,
				StockId = stockId
			});
		}
		else
		{
			ownership.Amount += amount;
		}
	}

	private void TakeStocksFromUser(Guid stockId, string userId, int amount)
	{
		var ownership = _ctx.GetStockOwnership(userId, stockId);

		if (ownership is null || ownership.Amount < amount)
			throw new InvalidOperationException("Seller does not have enough stocks");

		ownership.Amount -= amount;
	}

	private void AddTransactionLog(Guid stockId, string buyerId, string? sellerId, int amount)
	{
		_ctx.Add(new Transaction
		{
			StockId = stockId,
			BuyerId = buyerId,
			SellerId = sellerId,
			Amount = amount,
			Timestamp = DateTime.Now
		});
	}
}
