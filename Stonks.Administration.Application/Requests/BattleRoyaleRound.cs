using MediatR;
using Stonks.Administration.Application.Helpers;
using Stonks.Administration.Db;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Administration.Application.Requests;

public record BattleRoyaleRound : IRequest<Response>;

public class BattleRoyaleRoundHandler
{
	private readonly IDbWriter _writer;
	private readonly IStockEvaluator _evaluator;
	private readonly IStonksConfiguration _config;
	private readonly IStonksLogger<BattleRoyaleRoundHandler> _logger;

	private readonly IStockRepository _stock;
	private readonly IShareRepository _share;
	private readonly IOfferRepository _tradeOffer;

	public BattleRoyaleRoundHandler(IDbWriter writer,
		IStockEvaluator evaluator,
		IStonksConfiguration config,
		IStonksLogger<BattleRoyaleRoundHandler> logger,
		IStockRepository stock,
		IShareRepository share,
		IOfferRepository tradeOffer)
	{
		_writer = writer;
		_config = config;
		_logger = logger;
		_evaluator = evaluator;

		_stock = stock;
		_share = share;
		_tradeOffer = tradeOffer;
	}

	public async Task<Response> Handle(BattleRoyaleRound request,
		CancellationToken cancellationToken)
	{
		try
		{
			var transaction = _writer.BeginTransaction();
			await BattleRoyaleRound(cancellationToken);
			await _writer.CommitTransaction(transaction, cancellationToken);
			return Response.Ok();
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task BattleRoyaleRound(CancellationToken cancellationToken)
	{
		var id = await _evaluator.FindWeakest(cancellationToken);
		var amount = _config.NewStocksAfterRound();

		await _stock.Bankrupt(id);
		_share.RemoveShares(id);
		_tradeOffer.RemoveOffers(id);
		await _tradeOffer.AddPublicOffers(amount, cancellationToken);
	}
}
