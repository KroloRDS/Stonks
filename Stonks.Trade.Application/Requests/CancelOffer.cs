using MediatR;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record CancelOffer(Guid UserId, Guid OfferId) : IRequest<Response>;

public class CancelOfferHandler : IRequestHandler<CancelOffer, Response>
{
	private readonly IOfferRepository _offer;
	private readonly IDbWriter _writer;
	private readonly IStonksLogger _logger;

	public CancelOfferHandler(IOfferRepository offer,
		IDbWriter writer, ILogProvider logProvider)
	{
		_offer = offer;
		_writer = writer;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response> Handle(CancelOffer request,
		CancellationToken cancellationToken = default)
	{
		var id = request.OfferId;
		try
		{
			var offer = await _offer.Get(request.OfferId, cancellationToken);
			if (offer is null)
				return Response.BadRequest($"Offer with ID {id} does not exist");
			if (offer.WriterId != request.UserId)
				return Response.BadRequest($"You can only cancel your own offers");

			_offer.Cancel(id);
			await _writer.SaveChanges(cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}
}
