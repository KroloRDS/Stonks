using Moq;
using NUnit.Framework;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Tests.Services;

public class UserServiceTest
{
	private readonly Mock<IUserRepository> _user = new();
	private readonly UserService _service;

	public UserServiceTest() => _service = new UserService(_user.Object);

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	public void SettleMoney_NoOfferWriterId_ShouldThrow(OfferType offerType)
	{
		var offer = new TradeOffer
		{
			Id = Guid.NewGuid(),
			Price = 1M,
			Type = offerType,
			Amount = 1,
			StockId = Guid.NewGuid(),
			WriterId = null
		};

		Assert.ThrowsAsync<ArgumentNullException>(() =>
			_service.SettleMoney(Guid.NewGuid(), offer, 1));

		_user.VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	[TestCase(OfferType.PublicOfferring)]
	public void SettleMoney_ValidParameters_ShouldNotThrow(OfferType offerType)
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
		_service.SettleMoney(clientId, offer, 1).Wait();

		//Assert
		Verify(clientId, offerType == OfferType.Buy);
		if (offerType != OfferType.PublicOfferring)
			Verify(writerId!.Value, offerType == OfferType.Sell);

		_user.VerifyNoOtherCalls();
	}

	private void Verify(Guid userId, bool positive)
	{
		var balance = (decimal x) => positive ?
			x > decimal.Zero : x < decimal.Zero;

		_user.Verify(x => x.ChangeBalance(userId, It.Is<decimal>(x => balance(x)),
			It.IsAny<CancellationToken>()), Times.Once());
	}

	[TearDown]
	public void ResetMocks() => _user.Invocations.Clear();
}
