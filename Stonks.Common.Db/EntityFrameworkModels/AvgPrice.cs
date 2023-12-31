﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Common.Db.EntityFrameworkModels;

public class AvgPrice : HasId
{
	[Required]
	public Guid StockId { get; set; }
	[Required]
	public Stock Stock { get; set; }
	[Required]
	public DateTime DateTime { get; set; }

	public ulong SharesTraded { get; set; }

	[Column(TypeName = "decimal(8,2)")]
	public decimal Price { get; set; }
}
