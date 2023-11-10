using Stonks.Common.Db.EntityFrameworkModels;
using Stonks.Common.Utils.Services;

namespace Stonks.Common.Db;

public class DbLogProvider : ILogProvider
{
	private readonly AppDbContext _ctx;
	private readonly ICurrentTime _currentTime;

	public DbLogProvider(AppDbContext ctx, ICurrentTime currentTime)
	{
		_ctx = ctx;
		_currentTime = currentTime;
	}

	public void Log(string className, string message,
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
				Timestamp = _currentTime.Get()
			});
			_ctx.SaveChanges();
		}
		catch {}
	}
}
