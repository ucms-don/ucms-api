namespace Ucms.Application.Features.Stocks.MappingProfiles;

using AutoMapper;
using Ucms.Application.Features.Stocks.DTOs;
using Ucms.Domain.Entities;

public class StockProfile : Profile
{
    public StockProfile()
    {
        CreateMap<Stock, StockModel>();
    }
}
