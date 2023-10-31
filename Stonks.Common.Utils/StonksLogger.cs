using System.Text.Json;

namespace Stonks.Common.Utils;

public interface IStonksLogger
{
	void Log(string message);
	void Log(string message, object obj);
	void Log(Exception exception);
	void Log(Exception exception, object obj);
	void Log(string message, Exception exception, object obj);
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

	public void Log(string message) =>
		_logger.Log(_className, message, null, null);

	public void Log(string message, object obj) =>
		_logger.Log(_className, message, null, GetObjectDump(obj));

	public void Log(Exception exception) =>
		_logger.Log(_className, exception.Message, exception.ToString(), null);

	public void Log(Exception exception, object obj) =>
		_logger.Log(_className, exception.Message, exception.ToString(), GetObjectDump(obj));

	public void Log(string message, Exception exception, object obj) =>
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
