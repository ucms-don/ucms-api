namespace Ucms.Application.Features.Suppliers.DTOs;

public record SupplierModel(Guid Id,
                           string Name,
                           string NameRu,
                           string? NameEn,
                           string? NameKa,
                           string? Code);
