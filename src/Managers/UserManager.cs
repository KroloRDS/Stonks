using Stonks.Data;
using Stonks.Helpers;

namespace Stonks.Managers;
public class UserManager : IUserManager
{
	private readonly AppDbContext _ctx;
	private const int EMAIL_MAX_LENGTH = 64;

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
		var length = email.Length;

		if (length > EMAIL_MAX_LENGTH)
			throw new EmailTooLongException(EMAIL_MAX_LENGTH);

		var atIndex = CheckAlias(length, email);
		CheckDomain(length, atIndex, email);
	}

	private static void ThrowError(string email)
	{
		throw new InvalidEmailException(email);
	}

	private static int CheckAlias(int length, string email)
	{
		if (!char.IsLetterOrDigit(email[0]))
			ThrowError(email);

		var currentChar = 1;
		while (currentChar < length && email[currentChar] != '@')
		{
			if (!email[currentChar].IsAlphaNum() &&
				email[currentChar] != '.' &&
				email[currentChar] != '-' &&
				email[currentChar] != '_')
				ThrowError(email);

			currentChar++;
		}

		if (currentChar + 1 >= length)
			ThrowError(email);

		return currentChar;
	}

	private static void CheckDomain(int length, int atIndex, string email)
	{
		var currentChar = length - 1;
		var charsBetweenDots = 1;
		atIndex++;

		if (!char.IsLetter(email[currentChar]))
			ThrowError(email);

		while (currentChar != atIndex)
		{
			currentChar--;

			if (char.IsLetter(email[currentChar]))
			{
				charsBetweenDots++;
				continue;
			}

			if (email[currentChar] != '.')
			{
				ThrowError(email);
			}

			if (charsBetweenDots < 2) ThrowError(email);
			charsBetweenDots = 0;
		}

		if (charsBetweenDots == length - atIndex)
			ThrowError(email);
	}
}
