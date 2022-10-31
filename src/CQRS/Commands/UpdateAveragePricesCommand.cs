using MediatR;
using Stonks.Data;

namespace Stonks.CQRS.Commands;

public record UpdateAveragePricesCommand : IRequest;

public class UpdateAveragePricesCommandHandler :
    IRequestHandler<UpdateAveragePricesCommand>
{
    private readonly AppDbContext _ctx;
    private readonly IMediator _mediator;

    public UpdateAveragePricesCommandHandler(AppDbContext ctx,
        IMediator mediator)
    {
        _ctx = ctx;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(UpdateAveragePricesCommand request,
        CancellationToken cancellationToken)
    {
        await Task.WhenAll(_ctx.Stock
            .Where(x => !x.Bankrupt)
            .Select(x => _mediator.Send(
                new UpdateAveragePriceCommand(x.Id), cancellationToken)));
        return Unit.Value;
    }
}
