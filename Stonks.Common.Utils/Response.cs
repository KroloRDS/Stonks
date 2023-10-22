namespace Stonks.Common.Utils.Response;

public record Response(Kind Kind, string Message)
{
	public static Response Ok() => new(Kind.Ok, "OK");
	public static Response BadRequest(string message) =>
		new(Kind.BadRequest, message);
	public static Response Error(Exception e) =>
		new(Kind.ServerError, "Server error: " + e.Message);
}

public record Response<T>(Kind Kind, string Message, T? Value) where T : class
{
	public static Response<T> Ok(T value) => new(Kind.Ok, "OK", value);
	public static Response<T> BadRequest(string message) =>
		new(Kind.BadRequest, message, null);
	public static Response<T> Error(Exception e) =>
		new(Kind.ServerError, "Server error: " + e.Message, null);
};

public enum Kind
{
	Ok,
	BadRequest,
	ServerError
}
