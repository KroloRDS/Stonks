using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.Views.Models;
using Stonks.CQRS.Commands.Trade;

namespace Stonks.Controllers;

[Authorize]
public class OfferController : BaseController
{
	public OfferController(IMediator mediator, IStonksLogger logger,
		AppDbContext context) : base(mediator, logger, context) {}

	[HttpDelete]
	[Route("cancelOffer/{id}")]
	public async Task<bool> Cancel(
		Guid id, CancellationToken cancellationToken)
	{
		return await TryExecuteCommand(
			new CancelOfferCommand(id), cancellationToken);
	}

	[HttpPost]
	[Route("placeOffer")]
	public async Task<bool> Place([Bind("stockId,amount,offerType,price")]
		Guid stockId, int amount, OfferType offerType, decimal price,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId() ?? throw new Exception("Unauthorised");
		var command = new PlaceOfferCommand(stockId,
			userId, amount, offerType, price);
		return await TryExecuteCommand(command, cancellationToken);
	}

	[HttpPost]
	[Route("offers/{symbol}/buy")]
	public async Task<PlaceOfferViewModel> Buy(string symbol,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId() ?? throw new Exception("Unauthorised");
		return await GetPlaceOfferViewModel(userId,
			symbol, OfferType.Buy, cancellationToken);
	}

	[HttpPost]
	[Route("offers/{symbol}/sell")]
	public async Task<PlaceOfferViewModel> Sell(string symbol,
		CancellationToken cancellationToken)
	{
		var userId = GetUserId() ?? throw new Exception("Unauthorised");
		return await GetPlaceOfferViewModel(userId,
			symbol, OfferType.Sell, cancellationToken);
	}

	private async Task<PlaceOfferViewModel> GetPlaceOfferViewModel(Guid userId,
		string symbol, OfferType type, CancellationToken cancellationToken)
	{
		return new PlaceOfferViewModel
		{
			OfferType = type,
			StockId = await GetStockId(symbol, cancellationToken),
			StockSymbol = symbol,
			UserId = userId
		};
	}
}
