namespace Stonks.Common.Utils.Models;

public record Response(Kind Kind, string Message)
{
    public static Response Ok() => new(Kind.Ok, "OK");
    public static Response<T> Ok<T>(T value) => new(Kind.Ok, "OK", value);

    public static Response BadRequest(string message) =>
        new(Kind.BadRequest, message);

    public static Response Error(Exception e) =>
        new(Kind.ServerError, "Server error: " + e.Message);
}

public record Response<T>(Kind Kind, string Message, T? Value = default)
{
    public static implicit operator Response<T>(Response r) =>
        new(r.Kind, r.Message);
};

public enum Kind
{
    Ok,
    BadRequest,
    ServerError
}
