using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.DTOs;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record GetUserOffers(Guid UserId);

public class GetUserOffersHandler
{
	private readonly IOfferRepository _offers;
	private readonly IStockRepository _stocks;
	private readonly IStonksLogger<GetUserOffersHandler> _logger;

	public GetUserOffersHandler(IOfferRepository offers,
		IStockRepository stocks,
		IStonksLogger<GetUserOffersHandler> logger)
	{
		_offers = offers;
		_stocks = stocks;
		_logger = logger;
	}

	public async Task<Response<IEnumerable<OfferDTO>>> Handle(
		GetUserOffers request, CancellationToken cancellationToken)
	{
		try
		{
			var offers = GetOffers(request);
			return Response<IEnumerable<OfferDTO>>.Ok(offers);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response<IEnumerable<OfferDTO>>.Error(ex);
		}
	}

	private IEnumerable<OfferDTO> GetOffers(GetUserOffers request)
	{
		var buyOffers = _offers.GetUserBuyOffers(request.UserId);
		var sellOffers = _offers.GetUserSellOffers(request.UserId);
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
