using NUnit.Framework;
using Stonks.Administration.Application.Services;
using Stonks.Administration.Domain.Models;
using Stonks.Common.Utils.Models.Constants;

namespace Stonks.Administration.Tests.Services;

[TestFixture]
public class AveragePriceCalculatorTest
{
    [Test]
    public void UpdateAveragePrice_NoData()
    {
        (var price, var sharesTraded) = AveragePriceCalculator.FromTransactions(
            Enumerable.Empty<Transaction>(), null);
        Assert.Multiple(() =>
        {
            Assert.That(sharesTraded, Is.EqualTo(0UL));
            Assert.That(price, Is.EqualTo(Constants.STOCK_DEFAULT_PRICE));
        });
    }

    [Test]
    public void UpdateAveragePrice_NoNewTransactions()
    {
        //Arrange
        var avgPrice = new AvgPrice
        {
            SharesTraded = 100,
            Price = 5M
        };

        //Act
        (var price, var sharesTraded) = AveragePriceCalculator.FromTransactions(
            Enumerable.Empty<Transaction>(), avgPrice);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(avgPrice.SharesTraded, Is.EqualTo(sharesTraded));
            Assert.That(avgPrice.Price, Is.EqualTo(price));
        });
    }

    [Test]
    public void UpdateAveragePrice_OnlyNewTransactions()
    {
        //Arrange
        var transactions = new[]
        {
            new Transaction { Amount = 10, Price = 10M },
            new Transaction { Amount = 20, Price = 20M },
            new Transaction { Amount = 30, Price = 30M },
            new Transaction { Amount = 11, Price = 10M },
        };

        var expectedAmount = transactions.Sum(x => x.Amount);
        var expectedAverage = transactions.Sum(x => x.Amount * x.Price) / expectedAmount;

        //Act
        (var price, var sharesTraded) = AveragePriceCalculator.FromTransactions(
            transactions, null);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(expectedAmount, Is.EqualTo(sharesTraded));
            Assert.That(expectedAverage, Is.EqualTo(price));
        });
    }

    [Test]
    public void UpdateAveragePrice_OldAndNewTransactions()
    {
        //Arrange
        var avgPrice = new AvgPrice
        {
            SharesTraded = 100,
            Price = 5M
        };

        var transactions = new[]
        {
            new Transaction { Amount = 10, Price = 10M },
            new Transaction { Amount = 20, Price = 20M },
            new Transaction { Amount = 30, Price = 30M },
            new Transaction { Amount = 11, Price = 10M },
        };

        var expectedAmount = (ulong)transactions.Sum(x => x.Amount) + avgPrice.SharesTraded;
        var expectedAverage = (transactions.Sum(x => x.Amount * x.Price) +
            avgPrice.SharesTraded * avgPrice.Price) / expectedAmount;

        //Act
        (var price, var sharesTraded) = AveragePriceCalculator.FromTransactions(
            transactions, avgPrice);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(expectedAmount, Is.EqualTo(sharesTraded));
            Assert.That(expectedAverage, Is.EqualTo(price));
        });
    }
}
