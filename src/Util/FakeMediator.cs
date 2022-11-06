using MediatR;

namespace Stonks.Util;

public class FakeMediator : IMediator
{
	private readonly static NullReferenceException _exception = 
		new("Mediator instance was not initialised");

	public IAsyncEnumerable<TResponse> CreateStream<TResponse>
		(IStreamRequest<TResponse> r, CancellationToken t = default) =>
		throw _exception;

	public IAsyncEnumerable<object?> CreateStream(object o,
		CancellationToken t = default) => throw _exception;

	public Task Publish(object o,
		CancellationToken t = default) => throw _exception;

	public Task Publish<TNotification>(TNotification n,
		CancellationToken t = default) where TNotification : INotification =>
		throw _exception;

	public Task<TResponse> Send<TResponse>(IRequest<TResponse> r,
		CancellationToken t = default) => throw _exception;

	public Task<object?> Send(object o,
		CancellationToken t = default) => throw _exception;
}
