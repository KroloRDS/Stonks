using MediatR;
using PayPal.Api;
using Stonks.Data;
using Stonks.Models;
using Z.EntityFramework.Plus;

namespace Stonks.Requests.Commands.Bankruptcy;

public record EmitNewSharesCommand(int Amount) : IRequest;

public class EmitNewSharesCommandHandler :
	IRequestHandler<EmitNewSharesCommand>
{
	private readonly AppDbContext _ctx;

	public EmitNewSharesCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(EmitNewSharesCommand request,
		CancellationToken cancellationToken)
	{
		var amount = request.Amount;
		if (amount <= 0)
			throw new ArgumentOutOfRangeException(nameof(amount));

		var stocks = await _ctx.Stock
			.Where(x => !x.Bankrupt && x.PublicallyOfferredAmount < amount)
			.UpdateAsync(x => new Stock { PublicallyOfferredAmount = amount},
			cancellationToken);

		return Unit.Value;
	}
}
