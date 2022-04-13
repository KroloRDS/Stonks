namespace Stonks.Managers.Trade;
public interface IUserBalanceManager
{
	void ChangeBalance(Guid? userId, decimal? amount);
	void TransferMoney(Guid? payerId, Guid? recipientId, decimal? amount);
}
