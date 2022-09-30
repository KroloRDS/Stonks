using MediatR;
using Stonks.Data;
using Stonks.Helpers;
using Stonks.Models;

namespace Stonks.Requests.Commands.Trade;

public record CancelOfferCommand(Guid OfferId) : IRequest;

public class CancelOfferCommandHandler : IRequestHandler<CancelOfferCommand>
{
	private readonly AppDbContext _ctx;

	public CancelOfferCommandHandler(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public async Task<Unit> Handle(CancelOfferCommand request,
		CancellationToken cancellationToken)
	{
		var offer = await ValidateRequest(request);
		_ctx.TradeOffer.Remove(offer);
		return Unit.Value;
	}

	private async Task<TradeOffer> ValidateRequest(CancelOfferCommand request)
	{
		var offer = await _ctx.GetByIdAsync<TradeOffer>(request.OfferId);

		if (offer.Type is OfferType.PublicOfferring)
			throw new PublicOfferingException();

		return offer;
	}
}
