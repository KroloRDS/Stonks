﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class TradeOffer : HasId
{
	[Required]
	public Guid StockId { get; set; }
	public Stock Stock { get; set; }

	public string? WriterId { get; set; }
	public User? Writer { get; set; }

	[Required]
	public OfferType Type { get; set; }
	[Required]
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
