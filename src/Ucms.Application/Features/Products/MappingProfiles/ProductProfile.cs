namespace Ucms.Application.Features.Products.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Domain.Entities;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductModel>();
    }
}
