using MediatR;
using Stonks.Data;
using Stonks.Models;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BankruptCommand(Guid StockId) : IRequest;

public class BankruptCommandHandler : IRequestHandler<BankruptCommand>
{
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;

	public BankruptCommandHandler(AppDbContext ctx, IMediator mediator)
	{
		_ctx = ctx;
		_mediator = mediator;
	}

	public async Task<Unit> Handle(BankruptCommand request,
		CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetByIdAsync<Stock>(
			request.StockId, cancellationToken);
		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;

		await _mediator.Send(new RemoveAllOffersForStockCommand(
			request.StockId), cancellationToken);
		await _mediator.Send(new RemoveAllSharesCommand(request.StockId),
			cancellationToken);
		return Unit.Value;
	}
}
