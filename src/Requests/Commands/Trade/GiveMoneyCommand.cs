using MediatR;
using Stonks.Data;
using Stonks.ExtensionMethods;

namespace Stonks.Requests.Commands.Trade;

public record GiveMoneyCommand(Guid UserId, decimal Amount) : IRequest;

public class GiveMoneyCommandHandler :
	IRequestHandler<GiveMoneyCommand>
{
	private readonly AppDbContext _ctx;

	public GiveMoneyCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(GiveMoneyCommand request,
		CancellationToken cancellationToken)
	{
		var amount = request.Amount.AssertPositive();
		var user = await _ctx.GetUserAsync(request.UserId, cancellationToken);
		user.Funds += amount;
		return Unit.Value;
	}
}
