using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.Views.Models;

namespace Stonks.CQRS.Queries.Common;

public record GetStocksViewModelQuery(Guid UserId, DateTime? From = null)
	: IRequest<GetStocksViewModelResponse>;

public class GetStockViewModelQueryHandler :
	BaseQuery<GetStocksViewModelQuery, GetStocksViewModelResponse>
{
	public GetStockViewModelQueryHandler(
		ReadOnlyDbContext ctx, IMediator mediator) : base(ctx, mediator) { }

	public override async Task<GetStocksViewModelResponse> Handle(
		GetStocksViewModelQuery request, CancellationToken cancellationToken)
	{
		await _ctx.EnsureExist<User>(request.UserId, cancellationToken);
		var from = request.From ?? DateTime.Now.AddMonths(-1);
		var transactionsQuery = new GetTransactionsQuery
		{
			UserId = request.UserId,
			FromDate = from
		};
		var priceQuery = new GetStockPricesQuery
		{
			FromDate = from
		};

		var transactionsTask = _mediator.Send(
			transactionsQuery, cancellationToken);
		var pricesTask = _mediator.Send(priceQuery, cancellationToken);
		var stocks = await GetStocks(cancellationToken);
		var offers = await GetOffers(request.UserId, cancellationToken);
		var shares = await GetShares(request.UserId, cancellationToken);
		var transactions = (await transactionsTask).Transactions;
		var prices = (await pricesTask).Prices;

		var viewModels = (from stock in stocks
			from share in shares
				.Where(x => x.StockId == stock.Id)
				.DefaultIfEmpty()
			join offer in offers
				on stock.Id equals offer.StockId
				into offerGroup
			join transaction in transactions
				on stock.Id equals transaction.StockId
				into transactionGroup
			join price in prices
				on stock.Id equals price.StockId
				into priceGroup
			select new StockViewModel
			{
				StockName = stock.Name,
				StockSymbol = stock.Symbol,
				CurrentPrice = stock.Price,
				OwnedAmount = share?.Amount ?? 0,
				Offers = offerGroup,
				Prices = priceGroup,
				Transactions = transactionGroup
			}).ToList();

		foreach (var model in viewModels)
		{
			model.Profit = GetProfit(model.Transactions, request.UserId,
				model.CurrentPrice, model.OwnedAmount);
		}

		return new GetStocksViewModelResponse(viewModels);
	}

	private Task<List<Stock>> GetStocks(CancellationToken cancellationToken)
	{
		return _ctx.Stock.Where(x => !x.Bankrupt)
			.Select(x => new Stock
			{
				Id = x.Id,
				Name = x.Name,
				Symbol = x.Symbol,
				Price = x.Price
			}).ToListAsync(cancellationToken);
	}

	private Task<List<TradeOffer>> GetOffers(Guid userId,
		CancellationToken cancellationToken)
	{
		return _ctx.TradeOffer.Where(x => x.WriterId == userId)
			.Select(x => new TradeOffer
			{
				Amount = x.Amount,
				Price = x.Price,
				StockId = x.StockId,
				Type = x.Type
			}).ToListAsync(cancellationToken);
	}

	private Task<List<Share>> GetShares(Guid userId,
		CancellationToken cancellationToken)
	{
		return _ctx.Share.Where(x => x.OwnerId == userId)
			.Select(x => new Share
			{
				StockId = x.StockId,
				Amount = x.Amount
			}).ToListAsync(cancellationToken);
	}

	private static decimal GetProfit(IEnumerable<Transaction> transactions,
		Guid userId, decimal currentPrice, int ownedAmount)
	{
		var buy = transactions.Where(x => x.BuyerId == userId)
			.Sum(x => x.Price * x.Amount);
		var sell = transactions.Where(x => x.SellerId == userId)
			.Sum(x => x.Price * x.Amount);
		return sell - buy + currentPrice * ownedAmount;
	}
}
