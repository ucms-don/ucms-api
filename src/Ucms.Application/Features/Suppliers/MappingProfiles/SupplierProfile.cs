namespace Ucms.Application.Features.Suppliers.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Suppliers.DTOs;
using Ucms.Domain.Entities;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Supplier, SupplierModel>();
    }
}
