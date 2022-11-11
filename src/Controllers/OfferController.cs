﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Stonks.Util;
using Stonks.Data;
using Stonks.Data.Models;
using Stonks.Views.Models;
using Stonks.CQRS.Commands.Trade;
using System.Threading;

namespace Stonks.Controllers;

[Authorize]
public class OfferController : BaseController
{
	public OfferController(IMediator mediator, IStonksLogger logger,
		AppDbContext context) : base(mediator, logger, context) {}

	[Route("cancelOffer/{id}")]
	public async Task<IActionResult> Cancel(
		Guid id, CancellationToken cancellationToken)
	{
		return await TryExecuteCommand(
			new CancelOfferCommand(id), cancellationToken);
	}

	[Route("placeOffer")]
	[HttpPost]
	public async Task<IActionResult> Place([Bind("stockId,amount,offerType,price")]
		Guid stockId, int amount, OfferType offerType, decimal price,
		CancellationToken cancellationToken)
	{
		var command = new PlaceOfferCommand(stockId,
			GetUserId(), amount, offerType, price);
		return await TryExecuteCommand(command, cancellationToken);
	}

	[Route("offers/{symbol}/buy")]
	public async Task<IActionResult> Buy(string symbol,
		CancellationToken cancellationToken)
	{
		return View("Trade", await GetPlaceOfferViewModel(
			symbol, OfferType.Buy, cancellationToken));
	}

	[Route("offers/{symbol}/sell")]
	public async Task<IActionResult> Sell(string symbol,
		CancellationToken cancellationToken)
	{
		return View("Trade", await GetPlaceOfferViewModel(
			symbol, OfferType.Sell, cancellationToken));
	}

	private async Task<PlaceOfferViewModel> GetPlaceOfferViewModel(
		string symbol, OfferType type, CancellationToken cancellationToken)
	{
		return new PlaceOfferViewModel
		{
			OfferType = type,
			StockId = await GetStockId(symbol, cancellationToken),
			StockSymbol = symbol,
			UserId = GetUserId()
		};
	}
}
