namespace Stonks.Managers.PayPal;

public interface IUserPayPalManager
{
	void ChangePayPalEmail(Guid? userId, string? email);
}
