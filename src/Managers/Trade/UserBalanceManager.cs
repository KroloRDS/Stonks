using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

namespace Stonks.Managers.Trade;
public class UserBalanceManager : IUserBalanceManager
{
	private readonly AppDbContext _ctx;

	public UserBalanceManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void ChangeBalance(Guid? userId, decimal? amount)
	{
		if (amount is null) throw new ArgumentNullException(nameof(amount));
		ChangeBalance(_ctx.GetUser(userId), amount.Value);
		_ctx.SaveChanges();
	}

	private static void ChangeBalance(User user, decimal amount)
	{
		user.Funds += amount;
		if (user.Funds < 0) throw new InsufficientFundsException();
	}

	public void TransferMoney(Guid? payerId, Guid? recipientId, decimal? amount)
	{
		var amountValue = amount.AssertPositive();
		ChangeBalance(_ctx.GetUser(payerId), -amountValue);
		ChangeBalance(_ctx.GetUser(recipientId), amountValue);
		_ctx.SaveChanges();
	}
}
