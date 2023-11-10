using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stonks.Auth.Domain.Models;
using Stonks.Auth.Domain.Repositories;
using Stonks.Common.Db;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Auth.Db.Repositories;

public class UserRepository : IUserRepository
{
	private readonly IMapper _mapper;
	private readonly AppDbContext _writeCtx;
	private readonly ReadOnlyDbContext _readCtx;

	public UserRepository(IMapper mapper, AppDbContext writeCtx,
		ReadOnlyDbContext readCtx)
	{
		_mapper = mapper;
		_writeCtx = writeCtx;
		_readCtx = readCtx;
	}

	public async Task<bool> LoginExist(string login,
		CancellationToken cancellationToken = default) =>
		await _readCtx.User.AnyAsync(x => x.Login == login, cancellationToken);

	public async Task<User> GetUserFromLogin(string login,
		CancellationToken cancellationToken = default)
	{
		var user = await _readCtx.User.SingleAsync(x => 
			x.Login == login, cancellationToken);
		return _mapper.Map<User>(user);
	}

	public async Task Add(User user,
		CancellationToken cancellationToken = default)
	{
		await _writeCtx.AddAsync(
			_mapper.Map<EF.User>(user), cancellationToken);
	}
}
