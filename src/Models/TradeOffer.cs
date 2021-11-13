using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;

public class TradeOffer
{
	public int Id { get; set; }
	public Stock Stock { get; set; }
	public IdentityUser Writer { get; set; }
	public OfferType Type { get; set; }
	public int Amount { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal MaxPrice { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal MinPrice { get; set; }
}

public enum OfferType
{
	Buy = 0,
	Sell = 1,
	PublicOfferring = 2
}
