namespace Stonks.Trade.Application.DTOs;

public class OfferDTO
{
	public string Ticker { get; set; } = string.Empty;
	public int Amount { get; set; }
	public decimal Price { get; set; }
	public OfferType Type { get; set; }
}

public enum OfferType
{
	Buy,
	Sell
}
