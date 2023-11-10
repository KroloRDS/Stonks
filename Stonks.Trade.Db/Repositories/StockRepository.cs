using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;
using Stonks.Trade.Domain.Repositories;
using Stonks.Trade.Domain.Models;
using AutoMapper;

namespace Stonks.Trade.Db.Repositories;

public class StockRepository : IStockRepository
{
	private readonly IMapper _mapper;
	private readonly ReadOnlyDbContext _ctx;
	private readonly CommonRepositories.IStockRepository _stock;

	public StockRepository(IMapper mapper, ReadOnlyDbContext ctx,
		CommonRepositories.IStockRepository stock)
	{
		_ctx = ctx;
		_stock = stock;
		_mapper = mapper;
	}

	public async Task<bool> IsBankrupt(Guid stockId)
	{
		var stock = await _ctx.GetById<EF.Stock>(stockId);
		return stock is null || stock.BankruptDate is not null;
	}

	public async Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default) =>
		await _stock.LastBankruptDate(cancellationToken);

	public Dictionary<Guid, Stock> GetStockNames()
	{
		var stocks = _ctx.Stock.Select(_mapper.Map<Stock>);
		var dictionary = new Dictionary<Guid, Stock>();
		foreach (var stock in stocks)
			dictionary.Add(stock.Id, stock);

		return dictionary;
	}
}
