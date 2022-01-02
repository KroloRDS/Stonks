﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class HistoricalPrice : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }

	[Required]
	public DateTime DateTime { get; set; }

	public bool IsCurrent { get; set; }
	public ulong TotalAmountTraded { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal AveragePrice { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal PriceNormalised { get; set; }
}