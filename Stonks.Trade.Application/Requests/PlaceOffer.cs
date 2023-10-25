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
	private readonly IOfferRepository _offers;
	private readonly IShareRepository _shares;
	private readonly IStockRepository _stocks;
	private readonly IUserRepository _users;

	private readonly IDbWriter _writer;
	private readonly IOfferService _offerHelper;
	private readonly IStonksLogger _logger;

	public PlaceOfferHandler(IOfferRepository offers,
		IShareRepository shares,
		IStockRepository stocks,
		IUserRepository users,
		IDbWriter writer,
		IOfferService offerHelper,
		ILogProvider logProvider)
	{
		_offers = offers;
		_shares = shares;
		_stocks = stocks;
		_users = users;
		_writer = writer;
		_offerHelper = offerHelper;
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

		try
		{
			var transaction = _writer.BeginTransaction();
			await PlaceOffer(request, cancellationToken);
			await _writer.CommitTransaction(transaction, cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task ValidateRequest(PlaceOffer request,
		CancellationToken cancellationToken = default)
	{
		if (request.Type == OfferType.PublicOfferring)
			throw new PublicOfferingException();

		if (await _stocks.IsBankrupt(request.StockId))
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
		var ownedAmount = await _shares.GetOwnedAmount(writerId,
			stockId, cancellationToken);
		if (ownedAmount < amount)
			throw new NoStocksOnSellerException();
	}

	private async Task CheckAvailableFunds(decimal amount, Guid userId,
		CancellationToken cancellationToken = default)
	{
		var funds = _users.GetBalance(userId, cancellationToken);
		var offers = await _offers.GetUserBuyOffers(userId, cancellationToken);
		var inOtherOffers = offers.Sum(x => x.Price * x.Amount);

		if ((await funds) - inOtherOffers < amount)
			throw new InsufficientFundsException();
	}

	private async Task PlaceOffer(PlaceOffer request,
		CancellationToken cancellationToken = default)
	{
		// Try to match with existin offers first
		var offers = request.Type == OfferType.Buy ?
			await _offers.FindSellOffers(request.StockId,
				request.Price, cancellationToken) :
			await _offers.FindBuyOffers(request.StockId,
				request.Price, cancellationToken);
		
		var requestAmount = request.Amount;
		foreach (var offer in offers)
		{
			if (offer.Amount < requestAmount)
			{
				await _offerHelper.Accept(request.WriterId,
					offer.Id, offer.Amount, cancellationToken);
				requestAmount -= offer.Amount;
			}
			else
			{
				await _offerHelper.Accept(request.WriterId,
					offer.Id, requestAmount, cancellationToken);
				return;
			}
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

		await _offers.Add(newOffer, cancellationToken);
	}
}
