using Moq;
using NUnit.Framework;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Tests.Services;

[TestFixture]
public class OfferSerivceTest
{
	private readonly Mock<IOfferRepository> _offer = new();
	private readonly Mock<IUserService> _user = new();
	private readonly Mock<IShareService> _share = new();

	private OfferService _service
	{
		get => new(_offer.Object, _share.Object, _user.Object);
	}

	[Test]
	public void Accept_WrongOffer_ShouldThrow()
	{
		//Arrange
		_offer.Setup(x => x.Get(
			It.IsAny<Guid>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(null as TradeOffer);

		//Act & Assert
		Assert.ThrowsAsync<KeyNotFoundException>(() => 
			_service.Accept(new Guid(), new Guid(), 1));

		_user.VerifyNoOtherCalls();
		_share.VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void Accept_WrongAmount_ShouldThrow(int amount)
	{
		//Act & Assert
		Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_service.Accept(Guid.NewGuid(), Guid.NewGuid(), amount));

		_user.VerifyNoOtherCalls();
		_share.VerifyNoOtherCalls();
		_offer.VerifyNoOtherCalls();
	}

	[Test]
	[TestCase(1, 1)]
	[TestCase(2, 1)]
	[TestCase(1, 2)]
	[TestCase(100, 1)]
	[TestCase(1, 100)]
	public void Accept_ValidParameters_ShouldNotThrow(
		int amount, int offerAmount)
	{
		//Arrange
		_offer.Setup(x => x.Get(
			It.IsAny<Guid>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(new TradeOffer
			{
				Id = Guid.NewGuid(),
				Price = 1M,
				Type = OfferType.Buy,
				Amount = offerAmount,
				WriterId = Guid.NewGuid(),
			});

		//Act
		_service.Accept(Guid.NewGuid(), Guid.NewGuid(), amount).Wait();

		//Assert
		if (amount >= offerAmount)
			_offer.Verify(x => x.Cancel(It.IsAny<Guid>()), Times.Once());
		else
			_offer.Verify(x => x.DecreaseOfferAmount(
				It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());

		_offer.Verify(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
		_offer.VerifyNoOtherCalls();
	}

	[TearDown]
	public void ResetMocks()
	{
		_offer.Invocations.Clear();
		_share.Invocations.Clear();
		_user.Invocations.Clear();

		_offer.Reset();
	}
}
