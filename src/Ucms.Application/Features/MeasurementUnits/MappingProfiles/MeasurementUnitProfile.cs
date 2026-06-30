namespace Ucms.Application.Features.MeasurementUnits.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Domain.Entities;

public class MeasurementUnitProfile : Profile
{
    public MeasurementUnitProfile()
    {
        CreateMap<MeasurementUnit, MeasurementUnitModel>();
    }
}
