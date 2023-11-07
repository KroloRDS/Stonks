using NUnit.Framework;
using Stonks.Administration.Application.Services;

namespace Stonks.Administration.Tests.Services;

[TestFixture]
public class StockIndicatorsTest
{
	[Test]
	public void NormaliseStockIndicators_NotEnoughElements_ShouldReturnEmpty()
	{
		IEnumerable<StockIndicator> stocks1 = null!;
		var stocks2 = new List<StockIndicator>();

		Assert.Multiple(() =>
		{
			Assert.That(stocks1.Normalise(), Is.Not.Null);
			Assert.That(stocks1.Normalise().Any(), Is.False);
			Assert.That(stocks2.Normalise(), Is.Not.Null);
			Assert.That(stocks2.Normalise().Any(), Is.False);
		});
	}

	[Test]
	[TestCase(1)]
	[TestCase(5)]
	[TestCase(19)]
	public void NormaliseStockIndicators_SameElements_ShouldNormaliseToOne(int count)
	{
		//Arrange
		var fun = 0.12;
		var marketCap = -5432;
		var amount = 89;
		var volatility = -1.64;

		var stocks = new List<StockIndicator>();
		for (int i = 0; i < count; i++)
		{
			stocks.Add(new StockIndicator
			{
				Fun = fun,
				MarketCap = marketCap,
				StocksAmount = amount,
				Volatility = volatility
			});
		}

		//Act
		var actual = stocks.Normalise();

		//Assert
		foreach (var stock in actual)
		{
			Assert.Multiple(() =>
			{
				Assert.That(stock.Fun, Is.EqualTo(fun));
				Assert.That(stock.MarketCap, Is.EqualTo(1));
				Assert.That(stock.StocksAmount, Is.EqualTo(1));
				Assert.That(stock.Volatility, Is.EqualTo(1));
			});
		}
	}

	[Test]
	public void NormaliseStockIndicators_EvenlyDispersedElements()
	{
		//Arrange
		var fun = 0.12;
		var marketCap = 5432;
		var amount = 89;
		var volatility = 1.64;

		var stocks = new StockIndicator[]
		{
			new StockIndicator
			{
				Fun = fun,
				MarketCap = marketCap,
				StocksAmount = -amount,
				Volatility = -volatility
			},
			new StockIndicator
			{
				Fun = 0,
				MarketCap = 0,
				StocksAmount = 0,
				Volatility = 0
			},
			new StockIndicator
			{
				Fun = -fun,
				MarketCap = -marketCap,
				StocksAmount = amount,
				Volatility = volatility
			}
		};

		//Act
		var normalised = stocks.Normalise().ToList();
		var actual1 = normalised[0];
		var actual2 = normalised[1];
		var actual3 = normalised[2];

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual1.Fun, Is.EqualTo(fun));
			Assert.That(actual1.MarketCap, Is.EqualTo(1));
			Assert.That(actual1.StocksAmount, Is.EqualTo(1));
			Assert.That(actual1.Volatility, Is.EqualTo(1));

			Assert.That(actual2.Fun, Is.EqualTo(0));
			Assert.That(actual2.MarketCap, Is.EqualTo(0.5));
			Assert.That(actual2.StocksAmount, Is.EqualTo(0.5));
			Assert.That(actual2.Volatility, Is.EqualTo(0.5));

			Assert.That(actual3.Fun, Is.EqualTo(-fun));
			Assert.That(actual3.MarketCap, Is.EqualTo(0));
			Assert.That(actual3.StocksAmount, Is.EqualTo(0));
			Assert.That(actual3.Volatility, Is.EqualTo(0));
		});
	}

	[Test]
	public void NormaliseStockIndicators_QuaterWay()
	{
		//Arrange
		var fun = 0.12;

		var stocks = new StockIndicator[]
		{
			new StockIndicator
			{
				Fun = fun,
				MarketCap = 110,
				StocksAmount = -10,
				Volatility = -2
			},
			new StockIndicator
			{
				Fun = 2 * fun,
				MarketCap = 60,
				StocksAmount = 20,
				Volatility = -1
			},
			new StockIndicator
			{
				Fun = 3 * fun,
				MarketCap = -90,
				StocksAmount = 30,
				Volatility = 2
			}
		};

		//Act
		var normalised = stocks.Normalise().ToList();
		var actual1 = normalised[0];
		var actual2 = normalised[1];
		var actual3 = normalised[2];

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(actual1.Fun, Is.EqualTo(fun));
			Assert.That(actual1.MarketCap, Is.EqualTo(1));
			Assert.That(actual1.StocksAmount, Is.EqualTo(1));
			Assert.That(actual1.Volatility, Is.EqualTo(1));

			Assert.That(actual2.Fun, Is.EqualTo(2 * fun));
			Assert.That(actual2.MarketCap, Is.EqualTo(0.75));
			Assert.That(actual2.StocksAmount, Is.EqualTo(0.25));
			Assert.That(actual2.Volatility, Is.EqualTo(0.75));

			Assert.That(actual3.Fun, Is.EqualTo(3 * fun));
			Assert.That(actual3.MarketCap, Is.EqualTo(0));
			Assert.That(actual3.StocksAmount, Is.EqualTo(0));
			Assert.That(actual3.Volatility, Is.EqualTo(0));
		});
	}
}