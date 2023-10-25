using AutoMapper;
using Stonks.Auth.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Auth.Db;

public class MapperProfile : Profile
{
	public MapperProfile()
	{
		CreateMap<EF.User, User>();
		CreateMap<User, EF.User>();
		CreateMap<EF.Role, Role>();
		CreateMap<Role, EF.Role>();
	}
}
