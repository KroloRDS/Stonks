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

	public int BuyPublicallyOfferredStock(BuyStockDTO buyStockDTO)
	{
		var error = CheckBuyAmount(buyStockDTO);
		if (error != (int)Error.Code.OK)
			return error;

		var user = FindUser(buyStockDTO.BuyerId);
		if (user == null) return (int)Error.Code.CantFindUser;

		var stock = FindStock(buyStockDTO.StockId);
		if (stock == null) return (int)Error.Code.CantFindStock;

		var amount = buyStockDTO.Amount.Value;

		if (stock.PublicallyOfferredAmount < amount)
			return (int)Error.Code.NotEnoughPublicStocks;

		stock.PublicallyOfferredAmount -= amount;
		GiveStocksToUser(stock, user, amount);

		_ctx.SaveChanges();
		return (int)Error.Code.OK;
	}

	public int BuyStockFromUser(BuyStockDTO buyStockDTO)
	{
		var error = CheckBuyAmount(buyStockDTO);
		if (error != (int)Error.Code.OK)
			return error;

		var buyer = FindUser(buyStockDTO.BuyerId);
		if (buyer == null) return (int)Error.Code.CantFindUser;

		var seller = FindUser(buyStockDTO.BuyerId);
		if (seller == null) return (int)Error.Code.CantFindUser;

		var stock = FindStock(buyStockDTO.StockId);
		if (stock == null) return (int)Error.Code.CantFindStock;

		var amount = buyStockDTO.Amount.Value;
		
		error = TakeStocksFromUser(stock, seller, amount);
		if (error != (int)Error.Code.OK)
			return error;

		GiveStocksToUser(stock, buyer, amount);

		_ctx.SaveChanges();
		return (int)Error.Code.OK;
	}

	private int CheckBuyAmount(BuyStockDTO buyStockDTO)
	{
		if (buyStockDTO == null) return (int)Error.Code.NullParameter;
		if (buyStockDTO.Amount == null || buyStockDTO.Amount < 1)
			return (int)Error.Code.AmountNotPositive;

		return (int)Error.Code.OK;
	}

	private Stock? FindStock(Guid? id)
	{
		if (id == null) return null;
		return _ctx.Stock.FirstOrDefault(x => x.Id == id);
	}

	private IdentityUser? FindUser(Guid? id)
	{
		if (id == null) return null;
		return _ctx.Users.FirstOrDefault(x => x.Id == id.ToString());
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

	private int TakeStocksFromUser(Stock stock, IdentityUser user, int amount)
	{
		var ownership = _ctx.StockOwnership.FirstOrDefault(x =>
			x.Stock.Id == stock.Id &&
			x.Owner.Id == user.Id);

		if (ownership == null)
			return (int)Error.Code.NotEnoughUserStocks;

		if (ownership.Amount < amount)
			return (int)Error.Code.NotEnoughUserStocks;

		ownership.Amount -= amount;
		return (int)Error.Code.OK;
	}
}
