using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Models;
using Stonks.Providers;
using Stonks.ExtensionMethods;
using Stonks.Requests.Queries.Bankruptcy;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BattleRoyaleRoundCommand : IRequest;

public class BattleRoyaleRoundCommandHandler :
	IRequestHandler<BattleRoyaleRoundCommand>
{
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;
	private readonly IStonksConfiguration _config;

	private readonly AddPublicOffersHandler _addPublicOffers;

	public BattleRoyaleRoundCommandHandler(AppDbContext ctx,
		IMediator mediator, IStonksConfiguration config)
	{
		_ctx = ctx;
		_mediator = mediator;
		_config = config;

		_addPublicOffers = new AddPublicOffersHandler(ctx, mediator);
	}

	public async Task<Unit> Handle(BattleRoyaleRoundCommand request,
		CancellationToken cancellationToken)
	{
		var task = BattleRoyaleRound(cancellationToken);
		await _ctx.ExecuteTransactionAsync(task,
			nameof(BattleRoyaleRoundCommandHandler), cancellationToken);
		return Unit.Value;
	}

	public async Task BattleRoyaleRound(CancellationToken cancellationToken)
	{
		var weakestStock = _mediator.Send(
			new GetWeakestStockIdQuery(), cancellationToken);
		var amount = _config.NewStocksAfterRound();
		var id = (await weakestStock).Id;

		await Bankrupt(id, cancellationToken);
		await _addPublicOffers.Handle(amount, id, cancellationToken);
		await RemoveSharesAndOffers(id, cancellationToken);
		await UpdatePublicallyOfferedAmount(amount, id, cancellationToken);
	}

	public async Task Bankrupt(Guid id, CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetByIdAsync<Stock>(id);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;
	}

	public async Task RemoveSharesAndOffers(
		Guid id, CancellationToken cancellationToken)
	{
		await _ctx.EnsureExistAsync<Stock>(id, cancellationToken);
		await Task.Run(() => _ctx.Share.RemoveRange(
			_ctx.Share.Where(x => x.StockId == id)), cancellationToken);
		await Task.Run(() => _ctx.TradeOffer.RemoveRange(
			_ctx.TradeOffer.Where(x => x.StockId == id)), cancellationToken);
	}

	public async Task UpdatePublicallyOfferedAmount(int amount,
		Guid bankruptedId, CancellationToken cancellationToken)
	{
		amount.AssertPositive();
		await _ctx.Stock
			.Where(x => !x.Bankrupt && x.Id != bankruptedId && 
				x.PublicallyOfferredAmount < amount)
			.ForEachAsync(x => x.PublicallyOfferredAmount = amount,
			cancellationToken);
	}
}
