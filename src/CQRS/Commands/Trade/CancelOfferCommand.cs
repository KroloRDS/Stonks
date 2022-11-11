using MediatR;
using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.CQRS.Commands.Trade;

public record CancelOfferCommand(Guid OfferId) : IRequest;

public class CancelOfferCommandHandler : BaseCommand<CancelOfferCommand>
{
	public CancelOfferCommandHandler(AppDbContext ctx) : base(ctx) {}

	public override async Task<Unit> Handle(CancelOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await ValidateRequest(request);
        _ctx.TradeOffer.Remove(offer);
		await _ctx.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<TradeOffer> ValidateRequest(CancelOfferCommand request)
    {
        var offer = await _ctx.GetById<TradeOffer>(request.OfferId);

        if (offer.Type is OfferType.PublicOfferring)
            throw new PublicOfferingException();

        return offer;
    }
}
