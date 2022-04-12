namespace Stonks.Managers;
public interface IUserManager
{
	void ChangePayPalEmail(Guid? userId, string? email);
	void ChangeBalance(Guid? userId, decimal? amount);
	void TransferMoney(Guid? payerId, Guid? recipientId, decimal? amount);
}
