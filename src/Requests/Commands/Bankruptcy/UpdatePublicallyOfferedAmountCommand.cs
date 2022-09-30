using MediatR;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Helpers;

namespace Stonks.Requests.Commands.Bankruptcy;

public record UpdatePublicallyOfferedAmountCommand(int Amount) : IRequest;

public class UpdatePublicallyOfferedAmountCommandHandler :
	IRequestHandler<UpdatePublicallyOfferedAmountCommand>
{
	private readonly AppDbContext _ctx;

	public UpdatePublicallyOfferedAmountCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(UpdatePublicallyOfferedAmountCommand request,
		CancellationToken cancellationToken)
	{
		var amount = request.Amount.AssertPositive();
		await _ctx.Stock
			.Where(x => !x.Bankrupt && x.PublicallyOfferredAmount < amount)
			.ForEachAsync(x => x.PublicallyOfferredAmount = amount,
			cancellationToken);

		return Unit.Value;
	}
}
