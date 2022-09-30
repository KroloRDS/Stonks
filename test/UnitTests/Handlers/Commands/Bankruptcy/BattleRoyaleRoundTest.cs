using System;
using System.Threading;
using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Managers.Common;
using Stonks.Responses.Bankruptcy;
using Stonks.Requests.Queries.Bankruptcy;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class BattleRoyaleRoundTest : InMemoryDb
{
	private readonly Mock<IMediator> _mediator = new();
	private readonly Mock<IConfigurationManager> _config = new();
	private readonly BattleRoyaleRoundCommandHandler _handler;

	public BattleRoyaleRoundTest()
	{
		_mediator.Setup(x => x.Send(
			It.IsAny<GetWeakestStockIdQuery>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new GetWeakestStockIdResponse(Guid.NewGuid()));
		_handler = new BattleRoyaleRoundCommandHandler(_ctx,
			_mediator.Object, _config.Object);
	}

	[Test]
	public void BattleRoyaleRound_PositiveTest()
	{
		_handler.BattleRoyaleRound(CancellationToken.None).Wait();
		VerifyMediatorMocks();
	}

	private void VerifyMediatorMocks()
	{
		var token = It.IsAny<CancellationToken>();
		_mediator.Verify(x => x.Send(It.IsAny<GetWeakestStockIdQuery>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<AddPublicOffersCommand>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<UpdatePublicallyOfferedAmountCommand>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<BankruptCommand>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<RemoveAllOffersForStockCommand>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<RemoveAllSharesCommand>(),
			token), Times.Once());
		_mediator.VerifyNoOtherCalls();

		_config.Verify(x => x.NewStocksAfterRound(), Times.Once());
		_config.VerifyNoOtherCalls();
	}
}
