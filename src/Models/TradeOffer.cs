using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;

public class TradeOffer : HasId
{
	public Guid StockId { get; set; }
	public Stock Stock { get; set; }
	
	public string? WriterId { get; set; }
	public User? Writer { get; set; }

	public OfferType Type { get; set; }
	public int Amount { get; set; }

	[Column(TypeName = "decimal(8,2)")]
	public decimal BuyPrice { get; set; }

	[Column(TypeName = "decimal(8,2)")]
	public decimal SellPrice { get; set; }
}

public enum OfferType
{
	Buy,
	Sell,
	PublicOfferring
}
