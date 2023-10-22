using MediatR;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;
using Stonks.Trade.Application.Helpers;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Application.Requests;

public record PlaceOffer(
	Guid StockId,
	Guid WriterId,
	int Amount,
	OfferType Type,
	decimal Price
) : IRequest<Response>;

public class PlaceOfferHandler
{
	private readonly IOfferRepository _offers;
	private readonly IShareRepository _shares;
	private readonly IStockRepository _stocks;
	private readonly IUserRepository _users;

	private readonly IDbWriter _writer;
	private readonly IOfferHelper _offerHelper;
	private readonly IStonksLogger<PlaceOfferHandler> _logger;

	public PlaceOfferHandler(IOfferRepository offers,
		IShareRepository shares,
		IStockRepository stocks,
		IUserRepository users,
		IDbWriter writer,
		IOfferHelper offerHelper,
		IStonksLogger<PlaceOfferHandler> logger)
	{
		_offers = offers;
		_shares = shares;
		_stocks = stocks;
		_users = users;
		_writer = writer;
		_offerHelper = offerHelper;
		_logger = logger;
	}

	public async Task<Response> Handle(PlaceOffer request,
		CancellationToken cancellationToken)
	{
		ValidatedRequest validatedRequest;
		try
		{
			validatedRequest = await ValidateRequest(request, cancellationToken);
		}
		catch (Exception ex)
		{
			return Response.BadRequest(ex.Message);
		}

		try
		{
			var transaction = _writer.BeginTransaction();
			await PlaceOffer(validatedRequest, cancellationToken);
			await _writer.CommitTransaction(transaction, cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task<ValidatedRequest> ValidateRequest(
		PlaceOffer request, CancellationToken cancellationToken)
	{
		if (request.Type == OfferType.PublicOfferring)
			throw new PublicOfferingException();

		if (await _stocks.IsBankrupt(request.StockId))
			throw new BankruptStockException();

		var amount = request.Amount.AssertPositive();
		var price = request.Price.AssertPositive();

		if (request.Type == OfferType.Sell)
		{
			await CheckOwnedShares(request.WriterId, request.StockId,
				amount, cancellationToken);
		}
		if (request.Type == OfferType.Buy)
		{
			await CheckAvailableFunds(request.Price * request.Amount,
				request.WriterId, cancellationToken);
		}

		return new ValidatedRequest(request.Type, amount,
			request.StockId, price, request.WriterId);
	}

	private async Task CheckOwnedShares(Guid writerId, Guid stockId,
		int amount, CancellationToken cancellationToken)
	{
		var ownedAmount = await _shares.GetOwnedAmount(writerId,
			stockId, cancellationToken);
		if (ownedAmount < amount)
			throw new NoStocksOnSellerException();
	}

	private async Task CheckAvailableFunds(decimal amount,
		Guid userId, CancellationToken cancellationToken)
	{
		var funds = _users.GetBalance(userId, cancellationToken);
		var inOtherOffers = _offers.GetUserBuyOffers(userId)
			.Sum(x => x.Price * x.Amount);

		if ((await funds) - inOtherOffers < amount)
			throw new InsufficientFundsException();
	}

	private async Task PlaceOffer(ValidatedRequest request,
		CancellationToken cancellationToken)
	{
		// Try to match with existin offers first
		var offers = request.OfferType == OfferType.Buy ?
			_offers.FindSellOffers(request.StockId, request.Price) :
			_offers.FindBuyOffers(request.StockId, request.Price);

		foreach (var offer in offers)
		{
			if (offer.Amount < request.Amount)
			{
				await _offerHelper.Accept(request.WriterId,
					offer.Id, offer.Amount, cancellationToken);
				request.Amount -= offer.Amount;
			}
			else
			{
				await _offerHelper.Accept(request.WriterId,
					offer.Id, request.Amount, cancellationToken);
				return;
			}
		}

		// If there are still stocks to sell / buy
		var newOffer = new TradeOffer
		{
			Amount = request.Amount,
			StockId = request.StockId,
			WriterId = request.WriterId,
			Type = request.OfferType,
			Price = request.Price
		};

		await _offers.Add(newOffer, cancellationToken);
	}

	private record ValidatedRequest(OfferType OfferType, int Amount,
		Guid StockId, decimal Price, Guid WriterId)
	{
		public int Amount { get; set; } = Amount;
	}
}
