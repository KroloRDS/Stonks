using System.Text.Json;

namespace Stonks.Common.Utils;

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
	private readonly ILogProvider _logger;
	private readonly string _className;

	public StonksLogger(ILogProvider logger, string className)
	{
		_logger = logger;
		_className = className;
	}

	public bool Log(string message) =>
		_logger.Log(_className, message, null, null);

	public bool Log(string message, object obj) =>
		_logger.Log(_className, message, null, GetObjectDump(obj));

	public bool Log(Exception exception) =>
		_logger.Log(_className, exception.Message, exception.ToString(), null);

	public bool Log(Exception exception, object obj) =>
		_logger.Log(_className, exception.Message, exception.ToString(), GetObjectDump(obj));

	public bool Log(string message, Exception exception, object obj) =>
		_logger.Log(_className, message, exception.ToString(), GetObjectDump(obj));

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
