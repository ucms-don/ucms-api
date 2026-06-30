namespace Ucms.Application.Features.StockDemands.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Domain.Entities;

public class StockDemandItemProfile : Profile
{
    public StockDemandItemProfile()
    {
        CreateMap<StockDemandItem, StockDemandItemModel>();
    }
}
