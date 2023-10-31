using MediatR;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public class PlaceOffer : IRequest<Response>
{
	public Guid StockId { get; init; }
	public Guid WriterId { get; set; }
	public int Amount { get; init; }
	public OfferType Type { get; init; }
	public decimal Price { get; init; }
}

public class PlaceOfferHandler :
	IRequestHandler<PlaceOffer, Response>
{
	private readonly IOfferRepository _offer;
	private readonly IShareRepository _share;
	private readonly IStockRepository _stock;
	private readonly IUserRepository _user;

	private readonly IDbWriter _writer;
	private readonly IOfferService _offerService;
	private readonly IStonksLogger _logger;

	public PlaceOfferHandler(IOfferRepository offer,
		IShareRepository share,
		IStockRepository stock,
		IUserRepository user,
		IDbWriter writer,
		IOfferService offerService,
		ILogProvider logProvider)
	{
		_offer = offer;
		_share = share;
		_stock = stock;
		_user = user;
		_writer = writer;
		_offerService = offerService;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response> Handle(PlaceOffer request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await ValidateRequest(request, cancellationToken);
		}
		catch (Exception ex)
		{
			return Response.BadRequest(ex.Message);
		}

		var transaction = _writer.BeginTransaction();
		try
		{
			await PlaceOffer(request, cancellationToken);
			await _writer.CommitTransaction(transaction, cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_writer.RollbackTransaction(transaction);
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task ValidateRequest(PlaceOffer request,
		CancellationToken cancellationToken = default)
	{
		if (request.Type == OfferType.PublicOfferring)
			throw new PublicOfferingException();

		if (await _stock.IsBankrupt(request.StockId))
			throw new BankruptStockException();

		if (request.Amount < 1)
			throw new ArgumentOutOfRangeException(nameof(request.Amount));

		if (request.Price <= decimal.Zero)
			throw new ArgumentOutOfRangeException(nameof(request.Price));

		if (request.Type == OfferType.Sell)
		{
			await CheckOwnedShares(request.WriterId, request.StockId,
				request.Amount, cancellationToken);
		}
		if (request.Type == OfferType.Buy)
		{
			await CheckAvailableFunds(request.Price * request.Amount,
				request.WriterId, cancellationToken);
		}
	}

	private async Task CheckOwnedShares(Guid writerId, Guid stockId,
		int amount, CancellationToken cancellationToken = default)
	{
		var ownedAmount = await _share.GetOwnedAmount(stockId,
			writerId, cancellationToken);
		if (ownedAmount < amount)
			throw new NoStocksOnSellerException();
	}

	private async Task CheckAvailableFunds(decimal amount, Guid userId,
		CancellationToken cancellationToken = default)
	{
		var funds = _user.GetBalance(userId, cancellationToken);
		var offers = await _offer.GetUserBuyOffers(userId, cancellationToken);
		var inOtherOffers = offers.Sum(x => x.Price * x.Amount);

		if ((await funds) - inOtherOffers < amount)
			throw new InsufficientFundsException();
	}

	private async Task PlaceOffer(PlaceOffer request,
		CancellationToken cancellationToken = default)
	{
		// Try to match with existin offers first
		var offers = request.Type == OfferType.Buy ?
			await _offer.FindSellOffers(request.StockId,
				request.Price, cancellationToken) :
			await _offer.FindBuyOffers(request.StockId,
				request.Price, cancellationToken);
		
		var requestAmount = request.Amount;
		foreach (var offer in offers)
		{
			var acceptAmount = offer.Amount < requestAmount ?
				offer.Amount : requestAmount;

			await _offerService.Accept(request.WriterId,
					offer.Id, acceptAmount, cancellationToken);

			requestAmount -= acceptAmount;
			if (requestAmount == 0) return;
		}

		// If there are still stocks to sell / buy
		var newOffer = new TradeOffer
		{
			Amount = requestAmount,
			StockId = request.StockId,
			WriterId = request.WriterId,
			Type = request.Type,
			Price = request.Price
		};

		await _offer.Add(newOffer, cancellationToken);
	}
}
