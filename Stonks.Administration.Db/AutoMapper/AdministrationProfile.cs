using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Administration.Db.AutoMapper;
using Stonks.Administration.Domain.Models;
using EF = Stonks.Common.Db.EntityFrameworkModels;

namespace Stonks.Administration.Db.AutoMapper;

public class AdministrationProfile : Profile
{
    public AdministrationProfile()
    {
        CreateMap<EF.Stock, Stock>();
        CreateMap<EF.AvgPrice?, AvgPrice?>();
        CreateMap<EF.Transaction, Transaction>();
    }
}
