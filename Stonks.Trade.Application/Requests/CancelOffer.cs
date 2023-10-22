using MediatR;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record CancelOffer(Guid OfferId) : IRequest<Response>;

public class CancelOfferHandler
{
	private readonly IOfferRepository _offer;
	private readonly IDbWriter _writer;
	private readonly IStonksLogger<CancelOfferHandler> _logger;

	public CancelOfferHandler(IOfferRepository offer,
		IStonksLogger<CancelOfferHandler> logger,
		IDbWriter writer)
	{
		_offer = offer;
		_logger = logger;
		_writer = writer;
	}

	public Response Handle(CancelOffer request)
	{
		var id = request.OfferId;
		try
		{
			if (!_offer.Cancel(id))
				return Response.BadRequest($"Offer with ID {id} does not exist");

			_writer.SaveChanges();
			return Response.Ok();

		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}
}
