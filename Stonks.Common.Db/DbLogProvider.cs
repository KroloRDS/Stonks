using Stonks.Common.Db.EntityFrameworkModels;
using Stonks.Common.Utils;

namespace Stonks.Common.Db;

public class DbLogProvider : ILogProvider
{
	private readonly AppDbContext _ctx;

	public DbLogProvider(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public bool Log(string className, string message,
		string? exception, string? objectDump)
	{
		try
		{
			_ctx.Add(new Log
			{
				ClassName = className,
				Message = message,
				ObjectDump = objectDump,
				Exception = exception,
				Timestamp = DateTime.Now
			});
			_ctx.SaveChanges();
			return true;
		}
		catch
		{
			return false;
		}
	}
}
