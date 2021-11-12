namespace Stonks.Managers;

public interface ILogManager
{
	void Log(string message);
	void Log(Exception exception);
}