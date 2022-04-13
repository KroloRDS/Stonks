using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

[Index(nameof(Timestamp))]
public class Transaction : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	[Required]
	public string BuyerId { get; set; }
	[Required]
	public User Buyer { get; set; }

	public string? SellerId { get; set; }
	public User? Seller { get; set; }

	[Required]
	public int Amount { get; set; }
	[Required]
	public DateTime Timestamp { get; set; }
	[Required]
	[Column(TypeName = "decimal(8,2)")]
	public decimal Price { get; set; }
}
