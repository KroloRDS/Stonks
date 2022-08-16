using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record TransferMoneyCommand(
	Guid PayerId, Guid RecipientId, decimal Amount) : IRequest;

public class TransferMoneyCommandHandler :
	IRequestHandler<TransferMoneyCommand>
{
	public Task<Unit> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
