namespace Ucms.Application.Features.OrganizationMeasurementUnits.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.OrganizationMeasurementUnits.DTOs;
using Ucms.Domain.Entities;

public class OrganizationMeasurementUnitProfile : Profile
{
    public OrganizationMeasurementUnitProfile()
    {
        CreateMap<OrganizationMeasurementUnit, OrganizationMeasurementUnitModel>();
    }
}
