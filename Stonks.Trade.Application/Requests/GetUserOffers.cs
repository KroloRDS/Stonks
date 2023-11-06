using MediatR;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Application.DTOs;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record GetUserOffers(Guid UserId) : 
	IRequest<Response<IEnumerable<OfferDTO>>>;

public class GetUserOffersHandler :
	IRequestHandler<GetUserOffers, Response<IEnumerable<OfferDTO>>>
{
	private readonly IOfferRepository _offer;
	private readonly IStockRepository _stock;
	private readonly IStonksLogger _logger;

	public GetUserOffersHandler(IOfferRepository offer,
		IStockRepository stock, ILogProvider logProvider)
	{
		_offer = offer;
		_stock = stock;
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
		var buyOffers = await _offer.GetUserBuyOffers(
			request.UserId, cancellationToken);
		var sellOffers = await _offer.GetUserSellOffers(
			request.UserId, cancellationToken);
		var stocks = _stock.GetStockNames();

		var mappedOffers = buyOffers.Select(x => new OfferDTO
		{
			Amount = x.Amount,
			Price = x.Price,
			Type = OfferType.Buy,
			Ticker = stocks[x.StockId].Ticker
		}).ToList();

		mappedOffers.AddRange(sellOffers.Select(x => new OfferDTO
		{
			Amount = x.Amount,
			Price = x.Price,
			Type = OfferType.Sell,
			Ticker = stocks[x.StockId].Ticker
		}));
		return mappedOffers;
	}
}
