using MediatR;
using Stonks.Auth.Application.Services;
using Stonks.Auth.Db;
using Stonks.Auth.Domain.Models;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Auth.Application.Requests;

public record Register(string Login, string Password) :
	IRequest<Response<string>>;

public class RegisterHandler : IRequestHandler<Register, Response<string>>
{
	private readonly IUserRepository _user;
	private readonly IDbWriter _dbWriter;
	private readonly IStonksLogger _logger;
	private readonly IAuthService _auth;

	public RegisterHandler(IUserRepository user, IDbWriter dbWriter,
		ILogProvider logProvider, IAuthService auth)
	{
		_user = user;
		_dbWriter = dbWriter;
		_auth = auth;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response<string>> Handle(Register request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await Validate(request);
		}
		catch (Exception ex)
		{
			return Response.BadRequest(ex.Message);
		}

		try
		{
			var token = await Register(request, cancellationToken);
			return Response.Ok(token);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task Validate(Register request)
	{
		if (request?.Login is null)
			throw new ArgumentNullException(nameof(request.Login));

		if (request?.Password is null)
			throw new ArgumentNullException(nameof(request.Password));

		if (await _user.LoginExist(request.Login))
			throw new LoginAlreadyUsedException();
	}

	private async Task<string> Register(Register request,
		CancellationToken cancellationToken = default)
	{
		var rng = new Random();
		var salt = (short)rng.Next(short.MaxValue);
		var hash = AuthService.Hash(request.Password, salt);

		var user = new User
		{
			Id = Guid.NewGuid(),
			Login = request.Login,
			PasswordHash = hash,
			Salt = salt
		};
		await _user.Add(user, cancellationToken);
		await _dbWriter.SaveChanges(cancellationToken);

		var token = _auth.CreateAccessToken(user.Id,
			user.Login, Enumerable.Empty<string>());
		return token;
	}
}
