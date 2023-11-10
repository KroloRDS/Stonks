using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stonks.Administration.Domain.Models;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;
using CommonRepositories = Stonks.Common.Db.Repositories;
using Stonks.Common.Utils.Services;

namespace Stonks.Administration.Db.Repositories;

public class StockRepository : IStockRepository
{
	private readonly IMapper _mapper;
	private readonly ICurrentTime _currentTime;
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;
	private readonly CommonRepositories.IStockRepository _stock;

	public StockRepository(IMapper mapper,
		ICurrentTime currentTime,
		AppDbContext writeCtx,
		ReadOnlyDbContext readCtx,
		CommonRepositories.IStockRepository stock)
	{
		_mapper = mapper;
		_currentTime = currentTime;
		_writeCtx = writeCtx;
		_readCtx = readCtx;
		_stock = stock;
	}

	public async Task<Stock?> Get(Guid stockId)
	{
		var stock = await _readCtx.GetById<EF.Stock>(stockId);
		return stock is null ? null : _mapper.Map<Stock>(stock);
	}


	public async Task<IEnumerable<Stock>> GetActive(
		CancellationToken cancellationToken = default)
	{
		var stocks = await _readCtx.Stock.Where(x => x.BankruptDate == null)
			.ToListAsync(cancellationToken) ?? Enumerable.Empty<EF.Stock>();
		return stocks.Select(_mapper.Map<Stock>);
	}

	public async Task<DateTime?> LastBankruptDate(
		CancellationToken cancellationToken = default) =>
		await _stock.LastBankruptDate(cancellationToken);

	public async Task Bankrupt(Guid stockId)
	{
		var stock = await _writeCtx.GetById<EF.Stock>(stockId) ??
			throw new KeyNotFoundException($"Stock: {stockId}");
		stock.BankruptDate = _currentTime.Get();
	}
}
