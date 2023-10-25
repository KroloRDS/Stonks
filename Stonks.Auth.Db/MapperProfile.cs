using AutoMapper;
using Stonks.Auth.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Auth.Db;

public class MapperProfile : Profile
{
	public MapperProfile()
	{
		CreateMap<EF.User, User>();
		CreateMap<EF.Role, Role>();
	}
}
