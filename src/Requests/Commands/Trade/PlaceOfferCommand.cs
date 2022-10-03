using MediatR;
using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Models;
using Stonks.ExtensionMethods;
using Stonks.CustomExceptions;

namespace Stonks.Requests.Commands.Trade;

public record PlaceOfferCommand(
	Guid StockId,
	Guid WriterId,
	int Amount,
	OfferType Type,
	decimal Price
) : IRequest;

public class PlaceOfferCommandHandler : IRequestHandler<PlaceOfferCommand>
{
	private readonly AppDbContext _ctx;
	private readonly IMediator _mediator;

	public PlaceOfferCommandHandler(AppDbContext ctx,
		IMediator mediator)
	{
		_ctx = ctx;
		_mediator = mediator;
	}

	public async Task<Unit> Handle(PlaceOfferCommand placeOfferCommand,
		CancellationToken cancellationToken)
	{
		var request = await ValidateRequest(
			placeOfferCommand, cancellationToken);

		// Try to match with existin offers first
		var offers = request.OfferType == OfferType.Buy ?
			FindSellOffers(request.StockId, request.Price) :
			FindBuyOffers(request.StockId, request.Price);

		foreach (var offer in offers)
		{
			if (offer.Amount < request.Amount)
			{
				await _mediator.Send(new AcceptOfferCommand(request.WriterId,
					offer.Id), cancellationToken);
				request.Amount -= offer.Amount;
			}
			else
			{
				await _mediator.Send(new AcceptOfferCommand(request.WriterId,
					offer.Id, request.Amount), cancellationToken);
				return Unit.Value;
			}
		}

		// If there are still stocks to sell / buy
		var newOffer = new TradeOffer
		{
			Amount = request.Amount,
			StockId = request.StockId,
			WriterId = request.WriterId.ToString(),
			Type = request.OfferType,
			Price = request.Price
		};

		await _ctx.AddAsync(newOffer, cancellationToken);
		await _ctx.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}

	private async Task<ValidatedRequest> ValidateRequest(
		PlaceOfferCommand command, CancellationToken cancellationToken)
	{
		if (command.Type == OfferType.PublicOfferring)
			throw new PublicOfferingException();

		var stock = _ctx.GetById<Stock>(command.StockId);
		if (stock.Bankrupt)
			throw new BankruptStockException();

		var amount = command.Amount.AssertPositive();
		var price = command.Price.AssertPositive();
		var writerId = await _ctx.EnsureUserExistAsync(
			command.WriterId, cancellationToken);

		if (command.Type == OfferType.Sell)
		{
			await CheckOwnedShares(writerId, stock.Id,
				amount, cancellationToken);
		}
		if (command.Type == OfferType.Buy)
		{
			await CheckAvailableFunds(command.Price * command.Amount,
				writerId, cancellationToken);
		}

		return new ValidatedRequest(command.Type, amount,
			stock.Id, price, writerId);
	}

	private async Task CheckOwnedShares(Guid writerId, Guid stockId,
		int amount, CancellationToken cancellationToken)
	{
		var share = await _ctx.GetSharesAsync(writerId,
			stockId, cancellationToken);
		if (share?.Amount is null || share.Amount < amount)
			throw new NoStocksOnSellerException();
	}

	private async Task CheckAvailableFunds(decimal amount,
		Guid userId, CancellationToken cancellationToken)
	{
		var inOtherOffers = _ctx.TradeOffer
			.Where(x => x.Type == OfferType.Buy &&
				x.WriterId == userId.ToString())
			.SumAsync(x => x.Price * x.Amount, cancellationToken);

		var user = _ctx.GetUserAsync(userId, cancellationToken);

		if ((await user).Funds < await inOtherOffers + amount)
			throw new InsufficientFundsException();
	}

	private IEnumerable<TradeOffer> FindBuyOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
		   x.Type == OfferType.Buy &&
		   x.StockId == stockId &&
		   x.Price >= price)
			.OrderByDescending(x => x.Price)
			.ToList();
	}

	private IEnumerable<TradeOffer> FindSellOffers(Guid stockId, decimal price)
	{
		return _ctx.TradeOffer.Where(x =>
			x.Type != OfferType.Buy &&
			x.StockId == stockId &&
			x.Price <= price)
			.OrderBy(x => x.Price)
			.ToList();
	}

	private record ValidatedRequest(OfferType OfferType, int Amount,
		Guid StockId, decimal Price, Guid WriterId)
	{
		public int Amount { get; set; } = Amount;
	}
}
