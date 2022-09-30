using System;
using MediatR;
using NUnit.Framework;

using Stonks.Models;
using Stonks.Requests.Commands.Bankruptcy;

namespace UnitTests.Handlers.Commands.Bankruptcy;

public class UpdatePublicallyOfferedAmountTest :
	CommandTest<UpdatePublicallyOfferedAmountCommand>
{
	protected override IRequestHandler<
		UpdatePublicallyOfferedAmountCommand, Unit> GetHandler()
	{
		return new UpdatePublicallyOfferedAmountCommandHandler(_ctx);
	}

	[Test]
	[TestCase(0)]
	[TestCase(-2)]
	[TestCase(-99)]
	public void UpdatePublicallyOfferedAmount_WrongAmount_ShouldThrow(int amount)
	{
		AssertThrows<ArgumentOutOfRangeException>(
			new UpdatePublicallyOfferedAmountCommand(amount));
	}

	[Test]
	public void UpdatePublicallyOfferedAmount_PositiveTest()
	{
		//Arrange
		var amount = 100;
		var smallerAmount = 50;
		var biggerAmount = 150;

		//Check if test data makes sense
		Assert.Positive(amount);
		Assert.Positive(smallerAmount);
		Assert.Positive(biggerAmount);
		Assert.Greater(biggerAmount, amount);
		Assert.Greater(amount, smallerAmount);

		var stock1 = AddStock(0);
		var stock2 = AddStock(smallerAmount);
		var stock3 = AddStock(biggerAmount);
		var bankruptStock = AddBankruptStock();

		//Act
		Handle(new UpdatePublicallyOfferedAmountCommand(amount));

		//Assert
		Assert.AreEqual(amount, GetPublicAmount(stock1.Id));
		Assert.AreEqual(amount, GetPublicAmount(stock2.Id));
		Assert.AreEqual(biggerAmount, GetPublicAmount(stock3.Id));
		Assert.Zero(GetPublicAmount(bankruptStock.Id));
	}

	private int GetPublicAmount(Guid stockId)
	{
		return _ctx.GetById<Stock>(stockId).PublicallyOfferredAmount;
	}
}
