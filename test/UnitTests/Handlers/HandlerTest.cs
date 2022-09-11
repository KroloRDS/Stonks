using System;
using System.Threading;
using MediatR;
using NUnit.Framework;

namespace UnitTests.Handlers;

public abstract class HandlerTest<Request, Response> : InMemoryDb
	where Request : IRequest<Response>
{
	protected readonly IRequestHandler<Request, Response> _handler;

	public HandlerTest()
	{
		_handler = GetHandler();
	}

	protected abstract IRequestHandler<Request, Response> GetHandler();

	protected void AssertThrows<T>(Request request)
		where T : Exception
	{
		Assert.ThrowsAsync<T>(() => _handler.Handle(request, CancellationToken.None));
	}

	protected Response Handle(Request request)
	{
		return _handler.Handle(request, CancellationToken.None).Result;
	}
}
