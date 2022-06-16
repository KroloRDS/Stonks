namespace Stonks.Managers.Trade;
public interface IUserBalanceManager
{
	void TakeMoney(Guid? userId, decimal? amount);
	void GiveMoney(Guid? userId, decimal? amount);
	void TransferMoney(Guid? payerId, Guid? recipientId, decimal? amount);
}
