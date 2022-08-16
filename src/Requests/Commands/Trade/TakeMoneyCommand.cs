using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record TakeMoneyCommand(Guid UserId, decimal Amount) : IRequest;

public class TakeMoneyCommandHandler : IRequestHandler<TakeMoneyCommand>
{
	public Task<Unit> Handle(TakeMoneyCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
