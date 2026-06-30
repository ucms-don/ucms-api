namespace Ucms.Application.Features.Skus.MappingProfiles;

using AutoMapper;
using Ucms.Domain.Enums;
using Ucms.Domain.Entities;
using Ucms.Application.Features.Skus.DTOs;

public class SkuProfile : Profile
{
    public SkuProfile()
    {
        CreateMap<Sku, SkuModel>()
            .ForMember(dest => dest.ProductType, act => act.MapFrom(src => src.Product != null ? src.Product.Type : ProductType.Default))
            .ForMember(dest => dest.Product, act => act.MapFrom(src => src.Product));
    }
}
