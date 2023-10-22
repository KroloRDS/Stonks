using Microsoft.EntityFrameworkCore;
using Stonks.Common.Db;
using Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Db.Repositories;

public class StockRepository : IStockRepository
{
	private readonly ReadOnlyDbContext _ctx;
	private readonly CommonRepositories.IStockRepository _stock;

	public StockRepository(ReadOnlyDbContext ctx,
		CommonRepositories.IStockRepository stock)
	{
		_ctx = ctx;
		_stock = stock;
	}

	public async Task<bool> IsBankrupt(Guid stockId)
	{
		var stock = await _ctx.GetById<Stock>(stockId);
		return stock is null || stock.BankruptDate is not null;
	}

	public async Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default) =>
		await _stock.LastBankruptDate(cancellationToken);

	public async Task<Dictionary<Guid, string>> GetTickers(
		CancellationToken cancellationToken = default)
	{
		var stocks = await _ctx.Stock
			.Select(x => new Stock
			{
				Id = x.Id,
				Name = x.Name,
			}).ToListAsync(cancellationToken);

		var dictionary = new Dictionary<Guid, string>();
		foreach (var stock in stocks)
		{
			dictionary.Add(stock.Id, stock.Name);
		}
		return dictionary;
	}
}
