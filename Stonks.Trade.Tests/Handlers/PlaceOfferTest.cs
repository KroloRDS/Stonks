using Moq;
using NUnit.Framework;
using Stonks.Common.Db;
using Stonks.Common.Utils.Models;
using Stonks.Common.Utils.Models.Constants;
using Stonks.Common.Utils.Services;
using Stonks.Trade.Application.Requests;
using Stonks.Trade.Application.Services;
using Stonks.Trade.Db;
using Stonks.Trade.Domain.Models;
using Stonks.Trade.Domain.Repositories;

namespace Stonks.Trade.Tests.Handlers;

[TestFixture]
public class PlaceOfferTest
{
	private readonly Mock<IOfferRepository> _offer = new();
	private readonly Mock<IShareRepository> _share = new();
	private readonly Mock<IStockRepository> _stock = new();
	private readonly Mock<IUserRepository> _user = new();

	private readonly Mock<IDbWriter> _writer = new();
	private readonly Mock<IOfferService> _offerService = new();
	private readonly Mock<ILogProvider> _log = new();

	private PlaceOfferHandler _handler
	{
		get => new(_offer.Object,
			_share.Object,
			_stock.Object,
			_user.Object,
			_writer.Object,
			_offerService.Object,
			_log.Object);
	}

	[Test]
	public void PlaceOffer_WrongType_ShouldReturnBadRequest()
	{
		//Arange
		var request = new PlaceOffer
		{
			Amount = 1,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = Guid.NewGuid(),
			Type = OfferType.PublicOfferring
		};

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		AssertBadRequest<PublicOfferingException>(actual, false);
	}

	[Test]
	[TestCase(OfferType.Buy)]
	[TestCase(OfferType.Sell)]
	public void PlaceOffer_BankruptStock_ShouldReturnBadRequest(
		OfferType offerType)
	{
		//Arange
		var request = new PlaceOffer
		{
			Amount = 1,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = Guid.NewGuid(),
			Type = offerType
		};

		SetupBankrupt(true);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		AssertBadRequest<BankruptStockException>(actual);
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void PlaceOffer_WrongAmount_ShouldReturnBadRequest(int amount)
	{
		//Arange
		var request = new PlaceOffer
		{
			Amount = amount,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = Guid.NewGuid(),
			Type = OfferType.Buy
		};

		SetupBankrupt(false);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		AssertBadRequest<ArgumentOutOfRangeException>(actual);
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void PlaceOffer_WrongPrice_ShouldReturnBadRequest(decimal price)
	{
		//Arange
		var request = new PlaceOffer
		{
			Amount = 1,
			Price = price,
			StockId = Guid.NewGuid(),
			WriterId = Guid.NewGuid(),
			Type = OfferType.Buy
		};

		SetupBankrupt(false);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		AssertBadRequest<ArgumentOutOfRangeException>(actual);
	}

	[Test]
	public void PlaceSellOffer_NotEnoughShares_ShouldReturnBadRequest()
	{
		//Arrange
		var stockId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var ownedAmount = 5;
		var offerAmount = 10;
		Assert.That(offerAmount, Is.GreaterThan(ownedAmount));

		var request = new PlaceOffer
		{
			Amount = offerAmount,
			Price = decimal.One,
			StockId = stockId,
			WriterId = userId,
			Type = OfferType.Sell
		};

		SetupBankrupt(false);
		_share.Setup(x => x.GetOwnedAmount(stockId, userId,
			It.IsAny<CancellationToken>())).ReturnsAsync(ownedAmount);

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		_share.Verify(x => x.GetOwnedAmount(stockId, userId,
			It.IsAny<CancellationToken>()), Times.Once());
		AssertBadRequest<NoStocksOnSellerException>(actual);
	}

	[Test]
	public void PlaceBuyOffer_NoFunds_ShouldReturnBadRequest()
	{
		//Arrange
		var userId = Guid.NewGuid();
		var request = new PlaceOffer
		{
			Amount = 1,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = userId,
			Type = OfferType.Buy
		};

		SetupBankrupt(false);
		_user.Setup(x => x.GetBalance(userId,
			It.IsAny<CancellationToken>())).ReturnsAsync(0);
		_offer.Setup(x => x.GetUserBuyOffers(userId,
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Enumerable.Empty<TradeOffer>());

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		_user.Verify(x => x.GetBalance(userId,
			It.IsAny<CancellationToken>()), Times.Once());
		_offer.Verify(x => x.GetUserBuyOffers(userId,
			It.IsAny<CancellationToken>()), Times.Once());
		AssertBadRequest<InsufficientFundsException>(actual);
	}

	[Test]
	public void PlaceSellOffer_NoOtherOffers_ShouldAddOffer()
	{
		//Arrange
		var userId = Guid.NewGuid();
		var ownedAmount = 10;
		var offerAmount = 5;
		Assert.That(ownedAmount, Is.GreaterThan(offerAmount));

		var request = new PlaceOffer
		{
			Amount = offerAmount,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = userId,
			Type = OfferType.Sell
		};

		Setup(userId, ownedAmount, Enumerable.Empty<int>(),
			Enumerable.Empty<int>());

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
		VerifyAcceptAndAddOffers(OfferType.Sell, 0, true);
	}

	[Test]
	public void PlaceSellOffer_ExistingOffers_ShouldNotAddOffer()
	{
		//Arrange
		var userId = Guid.NewGuid();
		var sellAmount = 5;
		var buyAmount = 4;
		Assert.Multiple(() =>
		{
			Assert.That(sellAmount, Is.Positive);
			Assert.That(buyAmount, Is.Positive);
			Assert.That(buyAmount * 2, Is.GreaterThan(sellAmount));
		});

		var request = new PlaceOffer
		{
			Amount = sellAmount,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = userId,
			Type = OfferType.Sell
		};

		Setup(userId, sellAmount, new[]{ buyAmount, buyAmount },
			Enumerable.Empty<int>());

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
		VerifyAcceptAndAddOffers(OfferType.Sell, 2, false);
	}

	[Test]
	public void PlaceBuyOffer_NoOtherOffers_ShouldAddOffer()
	{
		//Arrange
		var userId = Guid.NewGuid();
		var request = new PlaceOffer
		{
			Amount = 1,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = userId,
			Type = OfferType.Buy
		};

		Setup(userId, 0, Enumerable.Empty<int>(),
			Enumerable.Empty<int>());

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
		VerifyAcceptAndAddOffers(OfferType.Buy, 0, true);
	}

	[Test]
	public void PlaceBuyOffer_ExistingOffers_ShouldNotAddOffer()
	{
		//Arrange
		var userId = Guid.NewGuid();
		var sellAmount = 5;
		var buyAmount = 7;
		Assert.Multiple(() =>
		{
			Assert.That(sellAmount, Is.Positive);
			Assert.That(buyAmount, Is.Positive);
			Assert.That(sellAmount * 2, Is.GreaterThan(buyAmount));
		});

		var request = new PlaceOffer
		{
			Amount = buyAmount,
			Price = decimal.One,
			StockId = Guid.NewGuid(),
			WriterId = userId,
			Type = OfferType.Buy
		};

		Setup(userId, 0, Enumerable.Empty<int>(),
			new[] { sellAmount, sellAmount });

		//Act
		var actual = _handler.Handle(request).Result;

		//Assert
		Assert.That(actual.Kind, Is.EqualTo(Kind.Ok));
		VerifyAcceptAndAddOffers(OfferType.Buy, 2, false);
	}

	private void AssertBadRequest<T>(Response? actual,
		bool verifyBankrupt = true) where T : Exception, new()
	{
		Assert.Multiple(() =>
		{
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual!.Kind, Is.EqualTo(Kind.BadRequest));
			Assert.That(actual.Message, Does.StartWith(new T().Message));
		});

		if (verifyBankrupt) VerifyBankrupt();
		VerifyNoOtherCalls();
	}

	private void SetupBankrupt(bool bankrupt) => _stock.Setup(x => 
		x.IsBankrupt(It.IsAny<Guid>())).ReturnsAsync(bankrupt);

	private void Setup(Guid userId, int userOwnedSharesAmount,
		IEnumerable<int> buyOfferAmounts, IEnumerable<int> sellOfferAmounts)
	{
		SetupBankrupt(false);
		_user.Setup(x => x.GetBalance(userId,
			It.IsAny<CancellationToken>())).ReturnsAsync(decimal.MaxValue);
		_offer.Setup(x => x.GetUserBuyOffers(userId,
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(Enumerable.Empty<TradeOffer>());

		_share.Setup(x => x.GetOwnedAmount(It.IsAny<Guid>(), userId,
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(userOwnedSharesAmount);

		_offer.Setup(x => x.FindBuyOffers(It.IsAny<Guid>(), It.IsAny<decimal>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(buyOfferAmounts.Select(x => new TradeOffer
			{
				Amount = x,
				Price = decimal.One,
				Type = OfferType.Buy
			}));

		_offer.Setup(x => x.FindSellOffers(It.IsAny<Guid>(),
			It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(sellOfferAmounts.Select(x => new TradeOffer
			{
				Amount = x,
				Price = decimal.One,
				Type = OfferType.Sell
			}));
	}

	private void VerifyBankrupt() => _stock.Verify(x =>
		x.IsBankrupt(It.IsAny<Guid>()), Times.Once());

	private void VerifyAcceptAndAddOffers(OfferType offerType,
		int acceptedOfferCount, bool addedOffer)
	{
		VerifyBankrupt();

		if (offerType == OfferType.Buy)
		{
			_user.Verify(x => x.GetBalance(It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()), Times.Once());
			_offer.Verify(x => x.GetUserBuyOffers(It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()), Times.Once());
			_offer.Verify(x => x.FindSellOffers(It.IsAny<Guid>(),
				It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once());
		}
		else
		{
			_share.Verify(x => x.GetOwnedAmount(It.IsAny<Guid>(),
				It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once());
			_offer.Verify(x => x.FindBuyOffers(It.IsAny<Guid>(),
				It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once());
		}

		_offerService.Verify(x => x.Accept(It.IsAny<Guid>(), It.IsAny<Guid>(),
			It.IsAny<int>(), It.IsAny<CancellationToken>()),
			Times.Exactly(acceptedOfferCount));

		if (addedOffer)
		{
			_offer.Verify(x => x.Add(It.IsAny<TradeOffer>(),
				It.IsAny<CancellationToken>()), Times.Once());
		}

		_writer.Verify(x => x.BeginTransaction(), Times.Once());
		_writer.Verify(x => x.CommitTransaction(It.IsAny<DbTransaction>(),
			It.IsAny<CancellationToken>()), Times.Once());

		VerifyNoOtherCalls();
	}

	private void VerifyNoOtherCalls()
	{
		_offer.VerifyNoOtherCalls();
		_share.VerifyNoOtherCalls();
		_stock.VerifyNoOtherCalls();
		_user.VerifyNoOtherCalls();
		_writer.VerifyNoOtherCalls();
		_offerService.VerifyNoOtherCalls();
		_log.VerifyNoOtherCalls();
	}

	[TearDown]
	public void ResetMocks()
	{
		_offer.Invocations.Clear();
		_share.Invocations.Clear();
		_stock.Invocations.Clear();
		_user.Invocations.Clear();
		_writer.Invocations.Clear();
		_offerService.Invocations.Clear();
		_log.Invocations.Clear();

		_offer.Reset();
		_share.Reset();
		_stock.Reset();
		_user.Reset();
		_writer.Reset();
		_offerService.Reset();
		_log.Reset();
	}
}
