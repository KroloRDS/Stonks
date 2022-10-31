using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using Stonks.CQRS.Queries.Bankruptcy.GetWeakestStock;

namespace UnitTests.CQRS.Queries.Bankruptcy;

public class StockIndicatorsTest
{
	[Test]
	public void NormaliseStockIndicators_NotEnoughElements_ShouldReturnEmpty()
	{
		IEnumerable<StockIndicator> stocks1 = null!;
		var stocks2 = new List<StockIndicator>();

		Assert.NotNull(StockIndicator.Normalise(stocks1));
		Assert.False(StockIndicator.Normalise(stocks1).Any());

		Assert.NotNull(StockIndicator.Normalise(stocks2));
		Assert.False(StockIndicator.Normalise(stocks2).Any());
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
		var actual = StockIndicator.Normalise(stocks);

		//Assert
		foreach (var stock in actual)
		{
			Assert.AreEqual(fun, stock.Fun);
			Assert.AreEqual(1, stock.MarketCap);
			Assert.AreEqual(1, stock.StocksAmount);
			Assert.AreEqual(1, stock.Volatility);
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
		var normalised = StockIndicator.Normalise(stocks).ToList();
		var actual1 = normalised[0];
		var actual2 = normalised[1];
		var actual3 = normalised[2];

		//Assert
		Assert.AreEqual(fun, actual1.Fun);
		Assert.AreEqual(1, actual1.MarketCap);
		Assert.AreEqual(1, actual1.StocksAmount);
		Assert.AreEqual(1, actual1.Volatility);

		Assert.AreEqual(0, actual2.Fun);
		Assert.AreEqual(0.5, actual2.MarketCap);
		Assert.AreEqual(0.5, actual2.StocksAmount);
		Assert.AreEqual(0.5, actual2.Volatility);

		Assert.AreEqual(-fun, actual3.Fun);
		Assert.AreEqual(0, actual3.MarketCap);
		Assert.AreEqual(0, actual3.StocksAmount);
		Assert.AreEqual(0, actual3.Volatility);
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
		var normalised = StockIndicator.Normalise(stocks).ToList();
		var actual1 = normalised[0];
		var actual2 = normalised[1];
		var actual3 = normalised[2];

		//Assert
		Assert.AreEqual(fun, actual1.Fun);
		Assert.AreEqual(1, actual1.MarketCap);
		Assert.AreEqual(1, actual1.StocksAmount);
		Assert.AreEqual(1, actual1.Volatility);

		Assert.AreEqual(2 * fun, actual2.Fun);
		Assert.AreEqual(0.75, actual2.MarketCap);
		Assert.AreEqual(0.25, actual2.StocksAmount);
		Assert.AreEqual(0.75, actual2.Volatility);

		Assert.AreEqual(3 * fun, actual3.Fun);
		Assert.AreEqual(0, actual3.MarketCap);
		Assert.AreEqual(0, actual3.StocksAmount);
		Assert.AreEqual(0, actual3.Volatility);
	}
}