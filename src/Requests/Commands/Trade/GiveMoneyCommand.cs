using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record GiveMoneyCommand(Guid UserId, decimal Amount) : IRequest;

public class GiveMoneyCommandHandler :
	IRequestHandler<GiveMoneyCommand>
{
	public Task<Unit> Handle(GiveMoneyCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
