using MediatR;
using Stonks.Data;

namespace Stonks.CQRS.Commands;

public record UpdateAveragePricesCommand : IRequest<Unit>;

public class UpdateAveragePricesCommandHandler :
    BaseCommand<UpdateAveragePricesCommand>
{
    public UpdateAveragePricesCommandHandler(AppDbContext ctx,
        IMediator mediator) : base(ctx, mediator) {}

    public override async Task<Unit> Handle(UpdateAveragePricesCommand request,
        CancellationToken cancellationToken)
    {
		await _ctx.ExecuteTransaction(UpdateAll(cancellationToken),
			nameof(UpdateAveragePricesCommandHandler), cancellationToken);
        return Unit.Value;
    }

	private async Task UpdateAll(CancellationToken cancellationToken)
	{
		var commands = _ctx.Stock.Where(x => !x.Bankrupt)
			.Select(x => new UpdateAveragePriceCommand(x.Id));
		foreach (var command in commands)
			await _mediator.Send(command, cancellationToken);
	}
}
