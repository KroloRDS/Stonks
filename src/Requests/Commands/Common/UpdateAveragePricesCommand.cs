using MediatR;

namespace Stonks.Requests.Commands.Common;

public record UpdateAveragePricesCommand : IRequest;

public class UpdateAveragePricesCommandHandler :
	IRequestHandler<UpdateAveragePricesCommand>
{
	public Task<Unit> Handle(UpdateAveragePricesCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
