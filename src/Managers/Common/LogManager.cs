using System.Text.Json;

using Stonks.Data;
using Stonks.Models;

namespace Stonks.Managers.Common;
public class LogManager : ILogManager
{
	private readonly AppDbContext _ctx;

	public LogManager(AppDbContext ctx)
	{
		_ctx = ctx;
	}

	public void Log(string message)
	{
		SaveLog(message, null, null);
	}

	public void Log(string message, object obj)
	{
		SaveLog(message, null, GetObjectDump(obj));
	}

	public void Log(Exception exception)
	{
		SaveLog(exception.Message, exception.ToString(), null);
	}

	public void Log(Exception exception, object obj)
	{
		SaveLog(exception.Message, exception.ToString(), GetObjectDump(obj));
	}

	public void Log(string message, Exception exception, object obj)
	{
		SaveLog(message, exception.ToString(), GetObjectDump(obj));
	}

	private void SaveLog(string message, string? exception, string? objectDump)
	{
		try
		{
			_ctx.Add(new Log
			{
				Message = message,
				ObjectDump = objectDump,
				Exception = exception,
				Timestamp = DateTime.Now
			});
			_ctx.SaveChanges();
		}
		catch
		{
			return;
		}
	}

	private static string GetObjectDump(object obj)
	{
		try
		{
			return JsonSerializer.Serialize(obj);
		}
		catch
		{
			return "";
		}
	}
}
