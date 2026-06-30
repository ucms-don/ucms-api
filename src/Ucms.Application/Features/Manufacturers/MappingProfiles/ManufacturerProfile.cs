namespace Ucms.Application.Features.Manufacturers.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Domain.Entities;

public class ManufacturerProfile : Profile
{
    public ManufacturerProfile()
    {
        CreateMap<Manufacturer, ManufacturerModel>();
    }
}
