using AutoMapper;
using Stonks.Administration.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Administration.Db;

public class MapperProfile : Profile
{
	public MapperProfile()
	{
		CreateMap<EF.Stock, Stock>();
		CreateMap<EF.User, User>();
		CreateMap<EF.AvgPrice, AvgPrice>();
		CreateMap<EF.Transaction, Transaction>();
		CreateMap<EF.Role, Role>();
	}
}
