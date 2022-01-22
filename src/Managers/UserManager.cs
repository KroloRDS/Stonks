using System.Text.RegularExpressions;

using Stonks.Data;
using Stonks.Helpers;

namespace Stonks.Managers;
public class UserManager : IUserManager
{
	private readonly AppDbContext _ctx;

	public UserManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void ChangePayPalEmail(Guid? userId, string? email)
	{
		var user = _ctx.GetUser(userId);
		email.AssertNotEmpty();
		email = email!.ToLower();
		ValidateEmail(email);

		//TODO: send confirmation email

		user.PayPalEmail = email;
		_ctx.SaveChanges();
	}

	private static void ValidateEmail(string email)
	{
		if (email.Length > 64)
			throw new ArgumentException("Email address is too long");

		var match = Regex.Match(email, @"^[a-z0-9.\-_]+@[a-z0-9\-]+\.[a-z0-9.\-]+$",
			RegexOptions.Singleline, TimeSpan.FromMilliseconds(10));

		if (!match.Success)
			throw new ArgumentException($"Address: \"{email}\" is not valid");
	}
}
