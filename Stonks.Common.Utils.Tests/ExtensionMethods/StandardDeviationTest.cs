﻿using NUnit.Framework;
using Stonks.Common.Utils.ExtensionMethods;

namespace Stonks.Common.Utils.Tests.ExtensionMethods;

[TestFixture]
public class StandardDeviationTest
{
    [Test]
    public void StandardDev_NotEnoughElements_ShouldReturnZero()
    {
        IEnumerable<decimal> seq1 = null!;
        var seq2 = new List<decimal>();
        var seq3 = new decimal[] { 1M };

        Assert.Multiple(() =>
        {
            Assert.That(seq1.StandardDev(), Is.EqualTo(0M));
            Assert.That(seq2.StandardDev(), Is.EqualTo(0M));
            Assert.That(seq3.StandardDev(), Is.EqualTo(0M));
        });
    }

    [Test]
    public void StandardDev_PositiveTest()
    {
        var seq = new decimal[] { -1M, 3M };
        Assert.That(seq.StandardDev(), Is.EqualTo(2M));
    }
}
