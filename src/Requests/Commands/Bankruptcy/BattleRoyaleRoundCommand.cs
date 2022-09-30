using MediatR;
using Stonks.Data;
using Stonks.Managers.Common;
using Stonks.Requests.Queries.Bankruptcy;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BattleRoyaleRoundCommand : IRequest;

public class BattleRoyaleRoundCommandHandler :
	IRequestHandler<BattleRoyaleRoundCommand>
{
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;
	private readonly IConfigurationManager _config;

	public BattleRoyaleRoundCommandHandler(
		AppDbContext ctx, IMediator mediator, IConfigurationManager config)
	{
		_ctx = ctx;
		_mediator = mediator;
		_config = config;
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

		var offers = _mediator.Send(
			new AddPublicOffersCommand(amount), cancellationToken);
		var shares = _mediator.Send(
			new UpdatePublicallyOfferedAmountCommand(amount), cancellationToken);
		var bankrupt = RemoveBankruptedStock((await weakestStock).Id,
			cancellationToken);

		await Task.WhenAll(bankrupt, shares, offers);
	}

	private async Task RemoveBankruptedStock(Guid id,
		CancellationToken cancellationToken)
	{
		var bankrupt = _mediator.Send(new BankruptCommand(
			id), cancellationToken);
		var removeOffers = _mediator.Send(new RemoveAllOffersForStockCommand(
			id), cancellationToken);
		var removeShares = _mediator.Send(new RemoveAllSharesCommand(
			id), cancellationToken);
		await Task.WhenAll(bankrupt, removeOffers, removeShares);
	}
}
