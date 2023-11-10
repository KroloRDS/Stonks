namespace Stonks.Trade.Domain.Models;

public class TradeOffer
{
	public Guid Id { get; set; }
	public Guid StockId { get; set; }

	public Guid? WriterId { get; set; }

	public OfferType Type { get; set; }

	public int Amount { get; set; }


	public decimal Price { get; set; }
}

public enum OfferType
{
	Buy,
	Sell,
	PublicOfferring
}
