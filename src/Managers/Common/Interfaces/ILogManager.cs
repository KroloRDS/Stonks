namespace Stonks.Managers.Common;

public interface ILogManager
{
	void Log(string message);
	void Log(string message, object obj);
	void Log(Exception exception);
	void Log(Exception exception, object obj);
	void Log(string message, Exception exception, object obj);
}