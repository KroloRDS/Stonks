using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;

public class Transaction : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	[Required]
	public string BuyerId { get; set; }
	[Required]
	public IdentityUser Buyer { get; set; }

	public string? SellerId { get; set; }
	public IdentityUser? Seller { get; set; }

	public int Amount { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal Price { get; set; }
}
