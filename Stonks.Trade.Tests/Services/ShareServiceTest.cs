using Moq;
using NUnit.Framework;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Tests.Services;

[TestFixture]
public class ShareServiceTest
{
	private readonly Mock<ICurrentTime> _currentTime = new();
	private readonly Mock<IShareRepository> _share = new();
	private readonly Mock<ITransactionRepository> _transaction = new();
	private readonly ShareService _service;

	public ShareServiceTest() => _service = new ShareService(
		_currentTime.Object, _share.Object, _transaction.Object);

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	public void Transfer_NoWriterId_ShouldThrow(OfferType offerType)
	{
		var offer = new TradeOffer { Type = offerType };
		Assert.ThrowsAsync<ArgumentNullException>(() =>
			_service.Transfer(Guid.NewGuid(), offer, 1));

		_currentTime.VerifyNoOtherCalls();
		_share.VerifyNoOtherCalls();
		_transaction.VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void Transfer_ValidParameters_ShouldNotThrow(OfferType offerType)
	{
		//Arrange
		var clientId = Guid.NewGuid();
		Guid? writerId = offerType == OfferType.PublicOfferring ?
			null : Guid.NewGuid();

		var offer = new TradeOffer
		{
			Id = Guid.NewGuid(),
			Price = 1M,
			Type = offerType,
			Amount = 1,
			StockId = Guid.NewGuid(),
			WriterId = writerId
		};

		//Act
		_service.Transfer(clientId, offer, 1).Wait();

		//Assert
		VerifyGive(offerType == OfferType.Buy ? writerId!.Value : clientId);
		if (offerType != OfferType.PublicOfferring)
			VerifyTake(offerType == OfferType.Buy ? clientId : writerId!.Value);

		_share.VerifyNoOtherCalls();
		_currentTime.Verify(x => x.Get(), Times.Once());
		_transaction.Verify(x => x.AddLog(It.IsAny<Transaction>(),
			It.IsAny<CancellationToken>()), Times.Once());
	}

	private void VerifyGive(Guid userId) =>
		_share.Verify(x => x.GiveSharesToUser(It.IsAny<Guid>(),
			userId, It.IsAny<int>(), It.IsAny<CancellationToken>()));

	private void VerifyTake(Guid userId) =>
		_share.Verify(x => x.TakeSharesFromUser(It.IsAny<Guid>(),
			userId, It.IsAny<int>(), It.IsAny<CancellationToken>()));

	[TearDown]
	public void ResetMocks()
	{
		_currentTime.Invocations.Clear();
		_share.Invocations.Clear();
		_transaction.Invocations.Clear();
	}
}
