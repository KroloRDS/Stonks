using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers;
public class LogManager : ILogManager
{
	private readonly AppDbContext _ctx;

	public LogManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void Log(string message)
	{
		_ctx.Add(new Log
		{
			Message = message,
			Timestamp = DateTime.Now
		});
		_ctx.SaveChanges();
	}

	public void Log(Exception exception)
	{
		_ctx.Add(new Log
		{
			Message = exception.Message,
			Details = exception.ToString(),
			Timestamp = DateTime.Now
		});
		_ctx.SaveChanges();
	}
}
