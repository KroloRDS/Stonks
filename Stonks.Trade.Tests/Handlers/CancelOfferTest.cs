using Moq;
using NUnit.Framework;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Application.Requests;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Tests.Handlers;

[TestFixture]
public class CancelOfferTest
{
	private readonly Mock<IOfferRepository> _offer = new();
	private readonly Mock<IDbWriter> _writer = new();
	private readonly Mock<ILogProvider> _log = new();

	private CancelOfferHandler _handler
	{
		get => new(_offer.Object, _writer.Object, _log.Object);
	}

	[Test]
	public void CancelOffer_WrongOffer_ShouldReturnBadRequest()
	{
		//Arrange
		var request = new CancelOffer(Guid.NewGuid(), Guid.NewGuid());

		_offer.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(null as TradeOffer);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
		Verify(false);
	}

	[Test]
	public void CancelOffer_WrongUser_ShouldReturnBadRequest()
	{
		//Arrange
		var request = new CancelOffer(Guid.NewGuid(), Guid.NewGuid());

		_offer.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new TradeOffer { WriterId = null });

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.BadRequest));
		Verify(false);
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	public void CancelOffer_ValidParameters_ShouldReturnOk(OfferType type)
	{
		//Arrange
		var userId = Guid.NewGuid();
		var request = new CancelOffer(userId, Guid.NewGuid());

		_offer.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new TradeOffer { WriterId = userId });

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
		Verify(true);
	}

	private void Verify(bool success)
	{
		_offer.Verify(x => x.Get(It.IsAny<Guid>(),
			It.IsAny<CancellationToken>()), Times.Once());

		if (success)
		{
			_offer.Verify(x => x.Cancel(It.IsAny<Guid>()), Times.Once());
			_writer.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()),
				Times.Once());
		}

		_offer.VerifyNoOtherCalls();
		_writer.VerifyNoOtherCalls();
		_log.VerifyNoOtherCalls();
	}

	[TearDown]
	public void ResetMocks()
	{
		_offer.Invocations.Clear();
		_writer.Invocations.Clear();
		_log.Invocations.Clear();

		_offer.Reset();
		_writer.Reset();
		_log.Reset();
	}
}
