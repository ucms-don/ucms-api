namespace Ucms.Application.Features.Outcomes.MappingProfiles;

using AutoMapper;
using Ucms.Domain.Enums;
using Ucms.Domain.Entities;
using Ucms.Application.Features.Outcomes.DTOs;

public class OutcomeItemProfile : Profile
{
    public OutcomeItemProfile()
    {
        CreateMap<OutcomeItem, OutcomeItemModel>()
            .ForMember(dest => dest.ProductType, act => act.MapFrom(src => src.Sku != null && src.Sku.Product != null ? src.Sku.Product.Type : ProductType.Default));
        CreateMap<OutcomeItem, CreateOutcomeItemModel>();
    }
}
