using Stonks.Data;
using Stonks.Helpers;

namespace Stonks.Managers.Trade;
public class UserBalanceManager : IUserBalanceManager
{
	private readonly AppDbContext _ctx;

	public UserBalanceManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void GiveMoney(Guid? userId, decimal? amount)
	{
		ChangeBalance(userId, amount.AssertPositive());
	}

	public void TakeMoney(Guid? userId, decimal? amount)
	{
		ChangeBalance(userId, -amount.AssertPositive());
	}

	public void TransferMoney(Guid? payerId, Guid? recipientId, decimal? amount)
	{
		TakeMoney(payerId, amount);
		GiveMoney(recipientId, amount);
	}

	private void ChangeBalance(Guid? userId, decimal amount)
	{
		var user = _ctx.GetUser(userId);
		user.Funds += amount;
		if (user.Funds < 0) throw new InsufficientFundsException();
	}
}
