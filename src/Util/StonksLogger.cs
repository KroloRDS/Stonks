using System.Text.Json;
using Stonks.Data;
using Stonks.Data.Models;

namespace Stonks.Util;

public interface IStonksLogger
{
	bool Log(string message);
	bool Log(string message, object obj);
	bool Log(Exception exception);
	bool Log(Exception exception, object obj);
	bool Log(string message, Exception exception, object obj);
}

public class StonksLogger : IStonksLogger
{
    private readonly AppDbContext _ctx;

    public StonksLogger(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public bool Log(string message)
    {
		return SaveLog(message, null, null);
    }

    public bool Log(string message, object obj)
    {
		return SaveLog(message, null, GetObjectDump(obj));
    }

    public bool Log(Exception exception)
    {
		return SaveLog(exception.Message, exception.ToString(), null);
    }

    public bool Log(Exception exception, object obj)
    {
		return SaveLog(exception.Message, exception.ToString(), GetObjectDump(obj));
    }

    public bool Log(string message, Exception exception, object obj)
    {
        return SaveLog(message, exception.ToString(), GetObjectDump(obj));
    }

    private bool SaveLog(string message, string? exception, string? objectDump)
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
			return true;
        }
        catch
        {
            return false;
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
