namespace Ucms.Application.Features.Skus.DTOs;

using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Application.Features.Suppliers.DTOs;
using Ucms.Domain.Enums;

public record SkuModel
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = default!;
    public ProductModel Product { get; set; } = default!;
    public ManufacturerModel Manufacturer { get; set; } = default!;
    public MeasurementUnitModel MeasurementUnit { get; set; } = default!;
    public SupplierModel Supplier { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public Guid MeasurementUnitId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public decimal StockSkuAmount { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public DateTimeOffset PurchaseDate { get; set; }
    public ProductType ProductType { get; set; }
    public SkuStatus Status { get; set; }

    /// <summary>
    /// Skladga material kiritilganda to'lov yechilgan kassa/bank hisobi (agar bog'langan bo'lsa).
    /// CashTransaction (SourceType=SkuPurchase) dan olinadi — edit oynasida preload uchun.
    /// </summary>
    public Guid? CashAccountId { get; set; }
}
