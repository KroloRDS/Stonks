using MediatR;
using Stonks.Data;
using Stonks.Models;

namespace Stonks.Requests.Commands.Bankruptcy;

public record BankruptCommand(Guid StockId) : IRequest;

public class BankruptCommandHandler : IRequestHandler<BankruptCommand>
{
	private readonly AppDbContext _ctx;

	public BankruptCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(BankruptCommand request,
		CancellationToken cancellationToken)
	{
		var stock = await _ctx.GetByIdAsync<Stock>(
			request.StockId);

		stock.Bankrupt = true;
		stock.BankruptDate = DateTime.Now;
		stock.PublicallyOfferredAmount = 0;
		return Unit.Value;
	}
}
