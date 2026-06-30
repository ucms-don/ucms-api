namespace Ucms.Application.Features.WorkTypes.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.WorkTypes.DTOs;
using Ucms.Domain.Entities;

public class WorkTypeProfile : Profile
{
    public WorkTypeProfile()
    {
        CreateMap<WorkType, WorkTypeModel>();
    }
}
