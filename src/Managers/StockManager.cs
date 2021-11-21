using Microsoft.AspNetCore.Identity;
using Stonks.Data;
using Stonks.DTOs;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers;

public class StockManager : IStockManager
{
	private readonly AppDbContext _ctx;

	//TODO: Ensure availability & safty for multiple threads
	public StockManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void BuyStock(BuyStockCommand? command)
	{
		(var stock, var user, var amount) = ValidateCommand(command);

		if (command.BuyFromUser != true)
		{
			if (stock.PublicallyOfferredAmount < amount)
				throw new InvalidOperationException("Not enough publically offered stocks");

			stock.PublicallyOfferredAmount -= amount;
			AddTransactionLog(stock, user, null, amount);
		}
		else
		{
			var seller = _ctx.GetUser(command.SellerId);
			TakeStocksFromUser(stock, seller, amount);
			AddTransactionLog(stock, user, seller, amount);
		}
		
		GiveStocksToUser(stock, user, amount);

		_ctx.SaveChanges();
	}

	private (Stock, IdentityUser, int) ValidateCommand(BuyStockCommand? command)
	{
		if (command is null)
			throw new ArgumentNullException(nameof(command));

		if (command.BuyFromUser != true && command.SellerId is not null)
			throw new ArgumentException("Reference to seller is not necessary when not buying from user", nameof(command));

		return (_ctx.GetById<Stock>(command.StockId),
			_ctx.GetUser(command.BuyerId),
			ValidationHelper.PositiveAmount(command.Amount));
	}

	private void GiveStocksToUser(Stock stock, IdentityUser user, int amount)
	{
		var ownership = _ctx.GetStockOwnership(user.Id, stock.Id);

		if (ownership is null)
		{
			_ctx.Add(new StockOwnership
			{
				Amount = amount,
				Owner = user,
				Stock = stock
			});
		}
		else
		{
			ownership.Amount += amount;
		}
	}

	private void TakeStocksFromUser(Stock stock, IdentityUser user, int amount)
	{
		var ownership = _ctx.GetStockOwnership(user.Id, stock.Id);

		if (ownership is null || ownership.Amount < amount)
			throw new InvalidOperationException("Seller does not have enough stocks");

		ownership.Amount -= amount;
	}

	private void AddTransactionLog(Stock stock, IdentityUser buyer, IdentityUser? seller, int amount)
	{
		_ctx.Add(new Transaction
		{
			Stock = stock,
			Buyer = buyer,
			Seller = seller,
			Amount = amount
		});
	}
}
