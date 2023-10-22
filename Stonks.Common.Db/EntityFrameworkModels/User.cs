using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stonks.Common.Db.EntityFrameworkModels;
public class User : IdentityUser<Guid>, IHasId
{
	[Column(TypeName = "decimal(11,2)")]
	public decimal Funds { get; set; }

	Guid IHasId.Id => Id;
}
