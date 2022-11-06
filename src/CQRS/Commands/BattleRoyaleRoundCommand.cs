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
    private readonly BattleRoyaleRoundRepository _repo;

    public BattleRoyaleRoundCommandHandler(AppDbContext ctx,
        IMediator mediator, IStonksConfiguration config) : base(ctx)
    {
        _repo = new BattleRoyaleRoundRepository(ctx, mediator, config,
            new AddPublicOffers(ctx, mediator));
    }

    public override async Task<Unit> Handle(BattleRoyaleRoundCommand request,
        CancellationToken cancellationToken)
    {
        var task = _repo.BattleRoyaleRound(cancellationToken);
        await _ctx.ExecuteTransaction(task,
            nameof(BattleRoyaleRoundCommandHandler), cancellationToken);
        return Unit.Value;
    }
}

public class BattleRoyaleRoundRepository
{
    private readonly AppDbContext _ctx;
    private readonly IMediator _mediator;
    private readonly IStonksConfiguration _config;
    private readonly IAddPublicOffers _addPublicOffers;

    public BattleRoyaleRoundRepository(AppDbContext ctx, IMediator mediator,
        IStonksConfiguration config, IAddPublicOffers addPublicOffers)
    {
        _ctx = ctx;
        _config = config;
        _mediator = mediator;
        _addPublicOffers = addPublicOffers;
    }

    public async Task BattleRoyaleRound(CancellationToken cancellationToken)
    {
        var weakestStock = _mediator.Send(
            new GetWeakestStockIdQuery(), cancellationToken);
        var amount = _config.NewStocksAfterRound();
        var id = (await weakestStock).Id;

        await Bankrupt(id);
        await _addPublicOffers.Handle(amount, id, cancellationToken);
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
