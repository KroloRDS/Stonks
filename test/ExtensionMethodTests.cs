using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stonks.Helpers;

namespace UnitTests;

[TestFixture]
public class ExtensionMethodTests
{
	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AssertPositiveInt_NegativeInput_ShouldThrow(int? amount)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => amount.AssertPositive());
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
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void AssertPositiveDecimal_NegativeInput_ShouldThrow(decimal? amount)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => amount.AssertPositive());
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
}
