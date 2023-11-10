using AutoMapper;
using Stonks.Trade.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Administration.Db;

public class TradeProfile : Profile
{
	public TradeProfile()
	{
		CreateMap<EF.AvgPrice, AvgPrice>();
		CreateMap<EF.Stock, Stock>();
		CreateMap<EF.TradeOffer, TradeOffer>();
		CreateMap<TradeOffer, EF.TradeOffer>();
	}
}
