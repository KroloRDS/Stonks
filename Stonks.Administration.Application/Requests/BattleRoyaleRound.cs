﻿using MediatR;
using Stonks.Administration.Application.Services;
using Stonks.Administration.Db;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Models.Configuration;
using Stonks.Common.Utils.Services;

namespace Stonks.Administration.Application.Requests;

public record BattleRoyaleRound : IRequest<Response>;

public class BattleRoyaleRoundHandler : 
	IRequestHandler<BattleRoyaleRound, Response>
{
	private readonly IDbWriter _writer;
	private readonly IStockEvaluator _evaluator;
	private readonly BattleRoyaleConfiguration _config;
	private readonly IStonksLogger _logger;

	private readonly IStockRepository _stock;
	private readonly IShareRepository _share;
	private readonly IOfferRepository _tradeOffer;

	public BattleRoyaleRoundHandler(IDbWriter writer,
		IStockEvaluator evaluator,
		BattleRoyaleConfiguration config,
		ILogProvider logProvider,
		IStockRepository stock,
		IShareRepository share,
		IOfferRepository tradeOffer)
	{
		_writer = writer;
		_config = config;
		_evaluator = evaluator;
		_logger = new StonksLogger(logProvider, GetType().Name);

		_stock = stock;
		_share = share;
		_tradeOffer = tradeOffer;
	}

	public async Task<Response> Handle(BattleRoyaleRound request,
		CancellationToken cancellationToken = default)
	{
		var transaction = _writer.BeginTransaction();
		try
		{
			await BattleRoyaleRound(cancellationToken);
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

	private async Task BattleRoyaleRound(
		CancellationToken cancellationToken = default)
	{
		var id = await _evaluator.FindWeakest(cancellationToken);
		var amount = _config.NewStocksAfterRound;

		await _stock.Bankrupt(id);
		_share.RemoveShares(id);
		_tradeOffer.RemoveOffers(id);
		_tradeOffer.SetExistingPublicOffersAmount(amount);
		await _tradeOffer.AddNewPublicOffers(amount, cancellationToken);
	}
}
