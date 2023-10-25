using MediatR;
using Stonks.Auth.Application.Services;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Auth.Application.Requests;

public record Login(string UserLogin, string Password) :
	IRequest<Response<Guid>>;

public class LoginHandler : IRequestHandler<Login, Response<Guid>>
{
	private readonly IUserRepository _user;
	private readonly IAuthService _authService;
	private readonly IStonksLogger _logger;

	public LoginHandler(IUserRepository user,
		IAuthService authService, ILogProvider logProvider)
	{
		_user = user;
		_authService = authService;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response<Guid>> Handle(Login request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return await Login(request, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response.Error(ex);
		}
	}

	private async Task<Response<Guid>> Login(Login request,
		CancellationToken cancellationToken = default)
	{
		var userExist = await _user.LoginExist(
			request.UserLogin, cancellationToken);
		if (!userExist)
			return Response.BadRequest("User does not exist");

		var user = await _user.GetUserFromLogin(
			request.UserLogin, cancellationToken);

		var hash = AuthService.Hash(request.Password, user.Salt);
		if (!hash.SequenceEqual(user.PasswordHash))
			return Response.BadRequest("Wrong password");

		var token = await _authService.RefreshToken(user.Id, cancellationToken);
		return Response.Ok(token);
	}
}
