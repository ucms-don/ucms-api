namespace Ucms.Application.Features.Outcomes.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Domain.Entities;

public class OutcomeProfile : Profile
{
    public OutcomeProfile()
    {
        CreateMap<Outcome, OutcomeModel>()
            .ForMember(dest => dest.IncomeStockId, src => src.MapFrom(m => m.IncomeOutcome != null ? m.IncomeOutcome.IncomeStockId : Guid.Empty))
            .ForMember(dest => dest.IncomeStock, src => src.MapFrom(m => m.IncomeOutcome != null ? m.IncomeOutcome.IncomeStock : null));
    }
}
