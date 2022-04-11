namespace Stonks.Helpers
{
	public class NoStocksToBankruptException : InvalidOperationException
	{
		private const string Desc = "No stocks to bankrupt";
		public NoStocksToBankruptException() : base(Desc) { }
	}

	public class NoStocksOnSellerException : InvalidOperationException
	{
		private const string Desc = "Seller does not have enough stocks";
		public NoStocksOnSellerException() : base(Desc) { }
	}

	public class NoPublicStocksException : InvalidOperationException
	{
		private const string Desc = "Not enough publically offered stocks";
		public NoPublicStocksException() : base(Desc) { }
	}

	public class BankruptStockException : InvalidOperationException
	{
		private const string Desc = "Invalid operation for bankrupt stock";
		public BankruptStockException() : base(Desc) { }
	}

	public class ExtraRefToSellerException : ArgumentException
	{
		private const string Desc = "Reference to seller is not necessary when not buying from user";
		public ExtraRefToSellerException() : base(Desc) { }
	}

	public class PlacingPublicOfferingException : ArgumentException
	{
		private const string Desc = "'Public offering' offer can only be placed by the broker";
		public PlacingPublicOfferingException() : base(Desc) { }
	}

	public class EmailTooLongException : ArgumentException
	{
		private const string Desc = "Email exceedes maximum allowed length of ";
		public EmailTooLongException(int length) : base(Desc + length) { }
	}

	public class InvalidEmailException : ArgumentException
	{
		private const string Desc = "Ivalid Email address: ";
		public InvalidEmailException(string email) : base(Desc + email) { }
	}
}
