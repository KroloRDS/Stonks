﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

[Index(nameof(Symbol), IsUnique = true)]
public class Stock : HasId
{
	[MaxLength(50)]
	public string Name { get; set; }

	[MaxLength(5)]
	public string Symbol { get; set; }

	[ConcurrencyCheck]
	public int PublicallyOfferredAmount { get; set; }

	[Column(TypeName = "decimal(15,9)")]
	public decimal Price { get; set; }
}
