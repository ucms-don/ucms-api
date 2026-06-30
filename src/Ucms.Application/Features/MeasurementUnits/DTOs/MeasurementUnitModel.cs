namespace Ucms.Application.Features.MeasurementUnits.DTOs;

using Ucms.Domain.Enums;

public record MeasurementUnitModel(
    Guid Id,
    string Name,
    string NameRu,
    string? NameEn,
    string? NameKa,
    string Code,
    decimal Multiplier,
    MeasurementUnitType Type);
