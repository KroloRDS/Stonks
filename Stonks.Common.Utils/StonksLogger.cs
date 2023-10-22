using System.Text.Json;

namespace Stonks.Common.Utils;

public interface IStonksLogger<T> where T : class
{
	bool Log(string message);
	bool Log(string message, object obj);
	bool Log(Exception exception);
	bool Log(Exception exception, object obj);
	bool Log(string message, Exception exception, object obj);
}

public class StonksLogger<T> : IStonksLogger<T> where T : class
{
	private readonly ILogProvider _logger;

	public StonksLogger(ILogProvider logger)
	{
		_logger = logger;
	}

	public bool Log(string message) =>
		_logger.Log(nameof(T), message, null, null);

	public bool Log(string message, object obj) =>
		_logger.Log(nameof(T), message, null, GetObjectDump(obj));

	public bool Log(Exception exception) =>
		_logger.Log(nameof(T), exception.Message, exception.ToString(), null);

	public bool Log(Exception exception, object obj) =>
		_logger.Log(nameof(T), exception.Message, exception.ToString(), GetObjectDump(obj));

	public bool Log(string message, Exception exception, object obj) =>
		_logger.Log(nameof(T), message, exception.ToString(), GetObjectDump(obj));

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
