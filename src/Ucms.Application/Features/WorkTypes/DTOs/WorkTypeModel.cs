namespace Ucms.Application.Features.WorkTypes.DTOs;

public record WorkTypeModel(
    Guid Id,
    string Name,
    string NameRu,
    string? NameEn,
    string? NameKa,
    Guid? MeasurementUnitId,
    string? Code);
