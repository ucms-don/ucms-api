namespace Ucms.Application.Features.StockSkus.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.StockSkus.DTOs;
using Ucms.Domain.Entities;

public class StockSkuProfile : Profile
{
    public StockSkuProfile()
    {
        CreateMap<StockSku, StockSkuModel>();
    }
}
