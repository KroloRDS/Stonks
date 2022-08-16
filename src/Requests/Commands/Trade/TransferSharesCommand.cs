using MediatR;

namespace Stonks.Requests.Commands.Trade;

public record TransferSharesCommand(
	Guid StockId,
	Guid BuyerId,
	Guid? SellerId,
	int Amount,
	bool BuyFromUser) : IRequest;

public class TransferSharesCommandHandler :
	IRequestHandler<TransferSharesCommand>
{
	public Task<Unit> Handle(TransferSharesCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
