using AutoMapper;
using Stonks.Auth.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Auth.Db.AutoMapper;

public class AuthProfile : Profile
{
	public AuthProfile()
	{
		CreateMap<EF.User, User>()
			.ForMember(dest => dest.Roles, src => src.MapFrom(
				x => FromComaSeparatedString(x.Roles)));
		CreateMap<User, EF.User>()
			.ForMember(dest => dest.Roles, src => src.MapFrom(
				x => FromEnum(x.Roles)));
	}

	private static IEnumerable<Role> FromComaSeparatedString(string roles) =>
		roles.Split(new[] { ',' }).Select(x => (Role)int.Parse(x));

	private static string FromEnum(IEnumerable<Role> roles) =>
		string.Join(',', roles.Select(x => (int)x).Order());
}
