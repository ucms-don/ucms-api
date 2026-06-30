namespace Ucms.Application.Features.Products.DTOs;

using Ucms.Domain.Enums;

public record ProductModel(Guid Id,
                           string Name,
                           string NameRu,
                           string? NameEn,
                           string? NameKa,
                           string? Code,
                           string? InternationalCode,
                           string? InternationalName,
                           string? AlternativeName,
                           ProductType Type);
