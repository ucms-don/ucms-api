namespace Ucms.Application.Features.Incomes.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Domain.Entities;

public class IncomeItemProfile : Profile
{
    public IncomeItemProfile()
    {
        CreateMap<IncomeItem, IncomeItemModel>();
    }
}
