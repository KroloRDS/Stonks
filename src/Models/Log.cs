using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class Log
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	public DateTime Timestamp { get; set; }

	[MaxLength(50)]
	public string? Message { get; set; }

	[MaxLength(1000)]
	public string? ObjectDump { get; set; }

	public string? Exception { get; set; }
}