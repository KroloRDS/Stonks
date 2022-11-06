using MediatR;
using Stonks.Data;

namespace Stonks.CQRS.Commands;

public record UpdateAveragePricesCommand : IRequest;

public class UpdateAveragePricesCommandHandler :
    BaseCommand<UpdateAveragePricesCommand>
{
    public UpdateAveragePricesCommandHandler(AppDbContext ctx,
        IMediator mediator) : base(ctx, mediator) {}

    public override async Task<Unit> Handle(UpdateAveragePricesCommand request,
        CancellationToken cancellationToken)
    {
        await Task.WhenAll(_ctx.Stock
            .Where(x => !x.Bankrupt)
            .Select(x => _mediator.Send(
                new UpdateAveragePriceCommand(x.Id), cancellationToken)));
        return Unit.Value;
    }
}
