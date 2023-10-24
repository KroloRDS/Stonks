using MediatR;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.DTOs;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record GetUserOffers(Guid UserId) : 
	IRequest<Response<IEnumerable<OfferDTO>>>;

public class GetUserOffersHandler :
	IRequestHandler<GetUserOffers, Response<IEnumerable<OfferDTO>>>
{
	private readonly IOfferRepository _offers;
	private readonly IStockRepository _stocks;
	private readonly IStonksLogger _logger;

	public GetUserOffersHandler(IOfferRepository offers,
		IStockRepository stocks, ILogProvider logProvider)
	{
		_offers = offers;
		_stocks = stocks;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response<IEnumerable<OfferDTO>>> Handle(
		GetUserOffers request, CancellationToken cancellationToken = default)
	{
		try
		{
			var offers = await GetOffers(request, cancellationToken);
			return Response.Ok(offers);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task<IEnumerable<OfferDTO>> GetOffers(GetUserOffers request,
		CancellationToken cancellationToken = default)
	{
		var buyOffers = await _offers.GetUserBuyOffers(
			request.UserId, cancellationToken);
		var sellOffers = await _offers.GetUserSellOffers(
			request.UserId, cancellationToken);
		var stocks = _stocks.GetStockNames();

		var mappedOffers = buyOffers.Select(x => new OfferDTO
		{
			Amount = x.Amount,
			Price = x.Price,
			Type = OfferType.Buy,
			Ticker = stocks[x.StockId].Ticker
		}).ToList();

		mappedOffers.AddRange(buyOffers.Select(x => new OfferDTO
		{
			Amount = x.Amount,
			Price = x.Price,
			Type = OfferType.Buy,
			Ticker = stocks[x.StockId].Ticker
		}));
		return mappedOffers;
	}
}
