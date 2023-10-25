using MediatR;
using Stonks.Auth.Application.Services;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Configuration;
using Stonks.Common.Utils.Response;

namespace Stonks.Auth.Application.Requests;

public record Login(string UserLogin, string Password) :
	IRequest<Response<string>>;

public class LoginHandler : IRequestHandler<Login, Response<string>>
{
	private readonly IUserRepository _user;
	private readonly IStonksLogger _logger;
	private readonly IAuthService _auth;

	public LoginHandler(IUserRepository user,
		ILogProvider logProvider, IAuthService auth)
	{
		_user = user;
		_auth = auth;
		_logger = new StonksLogger(logProvider, GetType().Name);
	}

	public async Task<Response<string>> Handle(Login request,
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

	private async Task<Response<string>> Login(Login request,
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

		var roles = user.Roles.Select(x => x.ToString());
		var token = _auth.CreateAccessToken(user.Id, user.Login, roles);
		return Response.Ok(token);
	}
}
