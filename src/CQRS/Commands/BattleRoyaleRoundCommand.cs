using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.CQRS.Helpers;
using Stonks.CQRS.Queries.Bankruptcy.GetWeakestStock;

namespace Stonks.CQRS.Commands;

public record BattleRoyaleRoundCommand : IRequest;

public class BattleRoyaleRoundCommandHandler :
    BaseCommand<BattleRoyaleRoundCommand>
{
	private readonly IStonksConfiguration _config;
	private readonly IAddPublicOffers _addOffers;

	public BattleRoyaleRoundCommandHandler(AppDbContext ctx,
        IMediator mediator, IStonksConfiguration config,
		IAddPublicOffers addOffers) : base(ctx, mediator)
    {
		_config = config;
		_addOffers = addOffers;
    }

    public override async Task<Unit> Handle(BattleRoyaleRoundCommand request,
        CancellationToken cancellationToken)
    {
        var task = BattleRoyaleRound(cancellationToken);
        await _ctx.ExecuteTransaction(task,
            nameof(BattleRoyaleRoundCommandHandler), cancellationToken);
        return Unit.Value;
    }

	private async Task BattleRoyaleRound(CancellationToken cancellationToken)
	{
		var weakestStock = _mediator.Send(
			new GetWeakestStockIdQuery(), cancellationToken);
		var amount = _config.NewStocksAfterRound();
		var id = (await weakestStock).Id;

		await Bankrupt(id);
		await _addOffers.Handle(amount, id, cancellationToken);
		await RemoveSharesAndOffers(id, cancellationToken);
		await UpdatePublicallyOfferedAmount(amount, id, cancellationToken);
	}

	public async Task Bankrupt(Guid id)
	{
		var stock = await _ctx.GetById<Stock>(id);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;
	}

	public async Task RemoveSharesAndOffers(
		Guid id, CancellationToken cancellationToken)
	{
		await _ctx.EnsureExist<Stock>(id, cancellationToken);
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
