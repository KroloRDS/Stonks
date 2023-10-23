using MediatR;
using Stonks.Administration.Application.Services;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Administration.Application.Requests;

public record Login(string UserLogin, string Password) :
	IRequest<Response<Guid>>;

public class LoginHandler
{
	private readonly IUserRepository _user;
	private readonly IAuthService _authService;
	private readonly IStonksLogger<LoginHandler> _logger;

	public LoginHandler(IUserRepository user, IAuthService authService,
		IStonksLogger<LoginHandler> logger)
	{
		_user = user;
		_authService = authService;
		_logger = logger;
	}

	public async Task<Response<Guid>> Handle(Login request,
		CancellationToken cancellationToken)
	{
		try
		{
			return await Login(request, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response<Guid>.Error(ex);
		}
	}

	private async Task<Response<Guid>> Login(Login request,
		CancellationToken cancellationToken)
	{
		var userExist = await _user.LoginExist(
			request.UserLogin, cancellationToken);
		if (!userExist)
			return Response<Guid>.BadRequest("User does not exist");

		var user = await _user.GetUserFromLogin(
			request.UserLogin, cancellationToken);

		var hash = AuthService.Hash(request.Password, user.Salt);
		if (!hash.SequenceEqual(user.PasswordHash))
			return Response<Guid>.BadRequest("Wrong password");

		var token = await _authService.RefreshToken(user.Id, cancellationToken);
		return Response<Guid>.Ok(token);
	}
}
