using MediatR;

namespace Stonks.Requests.Commands.Common;

public record UpdateAveragePriceForOneStockCommand(Guid StockId) : IRequest;

public class UpdateAveragePriceForOneStockCommandHandler :
	IRequestHandler<UpdateAveragePriceForOneStockCommand>
{
	public Task<Unit> Handle(UpdateAveragePriceForOneStockCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
