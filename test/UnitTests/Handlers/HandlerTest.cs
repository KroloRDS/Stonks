using System;
using System.Threading;
using Moq;
using MediatR;
using NUnit.Framework;
using Stonks.Managers.Common;

namespace UnitTests.Handlers;

public abstract class HandlerTest<Request, Response> : InMemoryDb
	where Request : IRequest<Response>
{
	protected readonly Mock<IMediator> _mediator = new();
	protected readonly Mock<IConfigurationManager> _config = new();
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

	[TearDown]
	public void ResetMockCallCounts()
	{
		_mediator.Invocations.Clear();
		_config.Invocations.Clear();
	}
}
