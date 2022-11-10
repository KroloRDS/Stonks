using System;
using System.Threading;
using Moq;
using MediatR;
using NUnit.Framework;

using Stonks.Util;
using Stonks.Data.Models;
using Stonks.CQRS.Queries.Common;
using Stonks.CQRS.Queries.Bankruptcy;
using Stonks.CQRS.Queries.Bankruptcy.GetWeakestStock;

namespace UnitTests.CQRS.Queries.Bankruptcy;

public class GetWeakestStockIdTest :
	QueryTest<GetWeakestStockIdQuery, GetWeakestStockIdResponse>
{
	protected override IRequestHandler<GetWeakestStockIdQuery,
		GetWeakestStockIdResponse> GetHandler()
	{
		SetupMediator();
		return new GetWeakestStockIdQueryHandler(
			_readOnlyCtx, _mediator.Object, _config.Object);
	}

	private void SetupMediator()
	{
		var token = It.IsAny<CancellationToken>();
		_mediator.Setup(x => x.Send(It.IsAny<GetTotalAmountOfSharesQuery>(),
			token)).ReturnsAsync(new GetTotalAmountOfSharesResponse(0));
		_mediator.Setup(x => x.Send(It.IsAny<GetPublicStocksAmountQuery>(),
			token)).ReturnsAsync(new GetPublicStocksAmountResponse(0));
		_mediator.Setup(x => x.Send(It.IsAny<GetLastBankruptDateQuery>(),
			token)).ReturnsAsync(new GetLastBankruptDateResponse(DateTime.Now));
		_mediator.Setup(x => x.Send(It.IsAny<GetStockPricesQuery>(),
			token)).ReturnsAsync(new GetStockPricesResponse(
				new[]{ new AvgPrice{ Price = 0 } }));
	}

	[Test]
	public void GetWeakestStockId_NoStocks_ShouldThrow()
	{
		var query = new GetWeakestStockIdQuery();
		AssertThrows<NoStocksToBankruptException>(query);
		
		AddBankruptStock();
		AssertThrows<NoStocksToBankruptException>(query);

		_config.VerifyNoOtherCalls();
		_mediator.VerifyNoOtherCalls();
	}

	[Test]
	public void GetWeakestStockId_PositiveTest()
	{
		//Arrange
		var stockId = AddStock().Id;
		AddBankruptStock();

		//Act
		var result = Handle(new GetWeakestStockIdQuery());

		//Assert
		Assert.AreEqual(stockId, result.Id);
		VerifyConfig();
		VerifyMediator();
	}

	private void VerifyMediator()
	{
		var token = It.IsAny<CancellationToken>();
		_mediator.Verify(x => x.Send(It.IsAny<GetPublicStocksAmountQuery>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<GetTotalAmountOfSharesQuery>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<GetLastBankruptDateQuery>(),
			token), Times.Once());
		_mediator.Verify(x => x.Send(It.IsAny<GetStockPricesQuery>(),
			token), Times.Once());
		_mediator.VerifyNoOtherCalls();
	}

	private void VerifyConfig()
	{
		_config.Verify(x => x.MarketCapWeight(), Times.Once());
		_config.Verify(x => x.StockAmountWeight(), Times.Once());
		_config.Verify(x => x.VolatilityWeight(), Times.Once());
		_config.Verify(x => x.FunWeight(), Times.Once());
		_config.VerifyNoOtherCalls();
	}
}
