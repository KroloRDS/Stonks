using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using Stonks.DTOs;
using Stonks.ExtensionMethods;

namespace UnitTests;

[TestFixture]
public class ExtensionMethodTest
{
	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AssertPositiveInt_NegativeInput_ShouldThrow(int? amount)
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => amount.AssertPositive());
		Assert.Throws<ArgumentOutOfRangeException>(
			() => amount!.Value.AssertPositive());
	}

	[Test]
	public void AssertPositiveInt_Null_ShouldThrow()
	{
		int? amount = null;
		Assert.Throws<ArgumentNullException>(() => amount.AssertPositive());
	}

	[Test]
	[TestCase(1)]
	[TestCase(5)]
	[TestCase(99)]
	public void AssertPositiveInt_PositiveTest(int? amount)
	{
		Assert.NotNull(amount);
		Assert.Greater(amount, 0);
		Assert.AreEqual(amount!.Value, amount.AssertPositive());
		Assert.AreEqual(amount!.Value, amount!.Value.AssertPositive());
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AssertPositiveDecimal_NegativeInput_ShouldThrow(decimal? amount)
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => amount.AssertPositive());

		Assert.Throws<ArgumentOutOfRangeException>(
			() => amount!.Value.AssertPositive());
	}

	[Test]
	public void AssertPositiveDecimal_Null_ShouldThrow()
	{
		int? amount = null;
		Assert.Throws<ArgumentNullException>(() => amount.AssertPositive());
	}

	[Test]
	[TestCase(1)]
	[TestCase(5)]
	[TestCase(99)]
	public void AssertPositiveDecimal_PositiveTest(decimal? amount)
	{
		Assert.NotNull(amount);
		Assert.Greater(amount, 0);
		Assert.AreEqual(amount!.Value, amount.AssertPositive());
		Assert.AreEqual(amount!.Value, amount!.Value.AssertPositive());
	}

	[Test]
	public void StandardDev_NotEnoughElements_ShouldReturnZero()
	{
		IEnumerable<decimal> seq1 = null!;
		var seq2 = new List<decimal>();
		var seq3 = new decimal[] { 1M };

		Assert.AreEqual(0M, seq1.StandardDev());
		Assert.AreEqual(0M, seq2.StandardDev());
		Assert.AreEqual(0M, seq3.StandardDev());
	}

	[Test]
	public void StandardDev_PositiveTest()
	{
		var seq = new decimal[] { -1M, 3M };
		Assert.AreEqual(2M, seq.StandardDev());
	}

	[Test]
	public void NormaliseStockIndicators_NotEnoughElements_ShouldReturnEmpty()
	{
		IEnumerable<StockIndicator> stocks1 = null!;
		var stocks2 = new List<StockIndicator>();

		Assert.False(stocks1.Normalise().Any());
		Assert.NotNull(stocks1.Normalise());

		Assert.False(stocks2.Normalise().Any());
		Assert.NotNull(stocks2.Normalise());
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
		var normalised = stocks.Normalise().ToList();
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
		var normalised = stocks.Normalise().ToList();
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
