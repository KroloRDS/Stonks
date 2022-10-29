using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Models;

public class Log
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	// Ordering by datetime is slow, so instead of adding index
	// we can order by the key, which is always indexed,
	// so we make it an auto-increment int instead of Guid,
	// to see the chronological order more easily.

	public DateTime Timestamp { get; set; }

	[MaxLength(150)]
	public string? Message { get; set; }

	[MaxLength(9999)]
	public string? ObjectDump { get; set; }

	public string? Exception { get; set; }
}