namespace Stonks.Managers;
public interface IUserManager
{
	void ChangePayPalEmail(Guid? userId, string? email);
}
