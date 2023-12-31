﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Stonks.Common.Db.EntityFrameworkModels;

[Index(nameof(Ticker), IsUnique = true)]
public class Stock : HasId
{
	[MaxLength(50)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(5)]
	public string Ticker { get; set; } = string.Empty;

	public DateTime? BankruptDate { get; set; }
}
