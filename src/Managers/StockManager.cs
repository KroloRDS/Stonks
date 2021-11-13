using Microsoft.AspNetCore.Identity;
using Stonks.Data;
using Stonks.DTOs;
using Stonks.Helpers;
using Stonks.Models;

namespace Stonks.Managers;

public class StockManager : IStockManager
{
	private readonly AppDbContext _ctx;

	public StockManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void BuyStock(BuyStockDTO buyStockDTO)
	{
		(var stock, var user, var amount) = ValidateDTO(buyStockDTO);

		if (buyStockDTO.BuyFromUser != true)
		{
			if (stock.PublicallyOfferredAmount < amount)
				throw new InvalidOperationException("Not enough publically offered stocks");

			stock.PublicallyOfferredAmount -= amount;
			AddTransactionLog(stock, user, null, amount);
		}
		else
		{
			var seller = FindUser(buyStockDTO.SellerId);
			TakeStocksFromUser(stock, seller, amount);
			AddTransactionLog(stock, user, seller, amount);
		}
		
		GiveStocksToUser(stock, user, amount);

		_ctx.SaveChanges();
	}

	private (Stock, IdentityUser, int) ValidateDTO(BuyStockDTO buyStockDTO)
	{
		if (buyStockDTO == null)
			throw new ArgumentNullException(nameof(buyStockDTO));

		return (FindStock(buyStockDTO.StockId),
			FindUser(buyStockDTO.BuyerId),
			CheckBuyAmount(buyStockDTO.Amount));
	}

	private static int CheckBuyAmount(int? amount)
	{
		if (amount == null)
			throw new ArgumentNullException(nameof(amount));

		if (amount < 1)
			throw new ArgumentOutOfRangeException(nameof(amount));

		return amount.Value;
	}

	private Stock FindStock(Guid? stockId)
	{
		if (stockId == null)
			throw new ArgumentNullException(nameof(stockId));

		var stock = _ctx.Stock.FirstOrDefault(x => x.Id == stockId);
		if (stock == null)
			throw new KeyNotFoundException(nameof(stockId));

		return stock;
	}

	private IdentityUser FindUser(Guid? userId)
	{
		if (userId == null)
			throw new ArgumentNullException(nameof(userId));

		var user = _ctx.Users.FirstOrDefault(x => x.Id == userId.ToString());
		if (user == null)
			throw new KeyNotFoundException(nameof(userId));

		return user;
	}

	private void GiveStocksToUser(Stock stock, IdentityUser user, int amount)
	{
		var ownership = _ctx.StockOwnership.FirstOrDefault(x =>
			x.Stock.Id == stock.Id &&
			x.Owner.Id == user.Id);

		if (ownership == null)
		{
			_ctx.StockOwnership.Add(new StockOwnership
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
		var ownership = _ctx.StockOwnership.FirstOrDefault(x =>
			x.Stock.Id == stock.Id &&
			x.Owner.Id == user.Id);

		if (ownership == null || ownership.Amount < amount)
			throw new InvalidOperationException("Seller does not have enough stocks");

		ownership.Amount -= amount;
	}

	private void AddTransactionLog(Stock stock, IdentityUser buyer, IdentityUser? seller, int amount)
	{
		_ctx.Transaction.Add(new Transaction
		{
			Stock = stock,
			Buyer = buyer,
			Seller = seller,
			Amount = amount
		});
	}
}
