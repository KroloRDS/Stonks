﻿using System;
using System.Threading;
using Moq;
using MediatR;
using NUnit.Framework;
using Stonks.Util;
using System.Transactions;

namespace UnitTests.CQRS;

public abstract class CQRSTest<Request, Response> : InMemoryDb
	where Request : IRequest<Response>
{
	protected readonly Mock<IMediator> _mediator = new();
	protected readonly Mock<IStonksConfiguration> _config = new();
	protected readonly IRequestHandler<Request, Response> _handler;

	public CQRSTest()
	{
		_handler = GetHandler();
	}

	protected abstract IRequestHandler<Request, Response> GetHandler();

	protected void AssertThrows<T>(Request request)
		where T : Exception
	{
		Assert.ThrowsAsync<T>(() =>
			_handler.Handle(request, CancellationToken.None));
	}

	protected void AssertThrowsInner<T>(Request request)
		where T : Exception
	{
		var ex = Assert.CatchAsync(() => 
			_handler.Handle(request, CancellationToken.None))
			?.InnerException;
		Assert.NotNull(ex);
		Assert.IsInstanceOf<T>(ex);
	}

	[TearDown]
	public void ResetMockCallCounts()
	{
		_mediator.Invocations.Clear();
		_config.Invocations.Clear();
	}
}