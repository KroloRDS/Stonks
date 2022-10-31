using System.Threading;
using MediatR;

namespace UnitTests.CQRS.Queries;

public abstract class QueryTest<Request, Response> : 
	CQRSTest<Request, Response> where Request : IRequest<Response>
{
    protected Response Handle(Request request)
    {
		return _handler.Handle(request, CancellationToken.None).Result;
	}
}
