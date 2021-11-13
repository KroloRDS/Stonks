using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Stonks.Models;

public class Transaction
{
	public Guid Id { get; set; }
	public Stock Stock { get; set; }
	public int Amount { get; set; }
	public IdentityUser Buyer { get; set; }
	public IdentityUser? Seller { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal Price { get; set; }
}
