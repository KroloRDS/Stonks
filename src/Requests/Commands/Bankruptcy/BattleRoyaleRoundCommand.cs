using MediatR;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BattleRoyaleRoundCommand : IRequest;

public class BattleRoyaleRoundCommandHandler :
	IRequestHandler<BattleRoyaleRoundCommand>
{
	public Task<Unit> Handle(BattleRoyaleRoundCommand request,
		CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
