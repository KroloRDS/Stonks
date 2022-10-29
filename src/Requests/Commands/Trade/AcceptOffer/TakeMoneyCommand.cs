using MediatR;
using Stonks.Data;
using Stonks.CustomExceptions;
using Stonks.ExtensionMethods;
using Stonks.Models;

namespace Stonks.Requests.Commands.Trade.AcceptOffer;

public record TakeMoneyCommand(Guid UserId, decimal Amount) : IRequest;

public class TakeMoneyCommandHandler : IRequestHandler<TakeMoneyCommand>
{
    private readonly AppDbContext _ctx;

    public TakeMoneyCommandHandler(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Unit> Handle(TakeMoneyCommand request,
        CancellationToken cancellationToken)
    {
        var amount = request.Amount.AssertPositive();
		var user = await _ctx.GetById<User>(request.UserId);
		user.Funds -= amount;
        if (user.Funds < 0) throw new InsufficientFundsException();
        return Unit.Value;
    }
}
