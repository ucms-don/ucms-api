namespace Ucms.Application.Features.Manufacturers.DTOs;

public record ManufacturerModel(Guid Id,
                                string Name,
                                string NameRu,
                                string? NameEn,
                                string? NameKa,
                                string? Code);
