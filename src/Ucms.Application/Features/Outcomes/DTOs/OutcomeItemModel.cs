namespace Ucms.Application.Features.Outcomes.DTOs;

using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Domain.Enums;

public record OutcomeItemModel
{
    public Guid Id { get; set; }
    public Guid SkuId { get; set; }
    public decimal Amount { get; set; }
    public decimal ActualAmount { get; set; }
    public Guid? MeasurementUnitId { get; set; }
    public ProductType ProductType { get; set; }
    public SkuModel? Sku { get; set; }
    public MeasurementUnitModel? MeasurementUnit { get; set; }
}
