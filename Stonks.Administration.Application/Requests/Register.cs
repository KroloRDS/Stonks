using MediatR;
using Stonks.Administration.Application.Services;
using Stonks.Administration.Domain.Repositories;
using Stonks.Common.Utils;
using Stonks.Common.Utils.Response;

namespace Stonks.Administration.Application.Requests;

public record Register(string Login, string Password) :
	IRequest<Response<Guid>>;

public class RegisterHandler
{
	private readonly IAuthService _authService;
	private readonly IUserRepository _user;
	private readonly IStonksLogger<RegisterHandler> _logger;

	public RegisterHandler(IAuthService authService, IUserRepository user,
		IStonksLogger<RegisterHandler> logger)
	{
		_authService = authService;
		_user = user;
		_logger = logger;
	}

	public async Task<Response<Guid>> Handle(Register request,
		CancellationToken cancellationToken)
	{
		try
		{
			await Validate(request);
		}
		catch (Exception ex)
		{
			return Response<Guid>.BadRequest(ex.Message);
		}

		try
		{
			var token = await Register(request, cancellationToken);
			return Response<Guid>.Ok(token);
		}
		catch (Exception ex)
		{
			_logger.Log(ex);
			return Response<Guid>.Error(ex);
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

	private async Task<Guid> Register(Register request,
		CancellationToken cancellationToken)
	{
		var rng = new Random();
		var salt = (short)rng.Next(short.MaxValue);
		var hash = AuthService.Hash(request.Password, salt);

		var userId = await _user.Add(
			request.Login,
			salt,
			hash,
			cancellationToken);
		var token = await _authService.RefreshToken(userId, cancellationToken);
		return token;
	}
}
