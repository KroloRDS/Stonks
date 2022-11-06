﻿namespace Stonks.Util;

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

public class InsufficientFundsException : InvalidOperationException
{
    private const string Desc = "User does not have sufficient funds";
    public InsufficientFundsException() : base(Desc) { }
}

public class PublicOfferingException : InvalidOperationException
{
    private const string Desc = "Invalid operation for 'Public Offering' trade offer";
    public PublicOfferingException() : base(Desc) { }
}

public class ExtraRefToSellerException : ArgumentException
{
    private const string Desc = "Reference to seller is not necessary when not buying from user";
    public ExtraRefToSellerException() : base(Desc) { }
}

public class DbTransactionException : SystemException
{
	public DbTransactionException(string handlerName, bool logged, Exception inner)
		: base(GetMessage(handlerName, logged), inner) {}

	private static string GetMessage(string handlerName, bool logged)
	{
		var msg = "Exception during transaction";
		msg += string.IsNullOrEmpty(handlerName) ? ". " :
			$"in {handlerName}. ";
		msg += logged ?
			"See inner exception for details." :
			"Failed to log inner exception.";
		return msg;
	}
}
