namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Smeta qatori — bir ish turi (e.g. "Стяжка пола", "Штукатурка стен")
/// </summary>
public class EstimateItem : Entity
{
    /// <summary>
    /// Smeta bo'limi ID
    /// </summary>
    public Guid SectionId { get; set; }

    /// <summary>
    /// Ish turi ID (WorkType ga FK)
    /// </summary>
    public Guid WorkTypeId { get; set; }

    public virtual WorkType? WorkType { get; set; }

    /// <summary>
    /// Qo'shimcha izoh / tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// O'lchov birligi ID (MeasurementUnit ga FK)
    /// </summary>
    public Guid MeasurementUnitId { get; set; }

    public virtual MeasurementUnit? MeasurementUnit { get; set; }

    /// <summary>
    /// Smeta bo'yicha umumiy hajm (zakazchik bilan kelishilgan)
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Birlik narhi — zakazchik bilan (so'm)
    /// </summary>
    public decimal ClientUnitPrice { get; set; }

    /// <summary>
    /// Birlik narhi — brigada uchun (so'm), odatiy qiymat
    /// </summary>
    public decimal BrigadeUnitPrice { get; set; }

    /// <summary>
    /// Material narhi — bir o'lchov birligi uchun (so'm)
    /// </summary>
    public decimal MaterialUnitPrice { get; set; }

    /// <summary>
    /// Tartib raqami
    /// </summary>
    public int Order { get; set; }

    public virtual EstimateSection? Section { get; set; }
    public virtual ICollection<WorkLog> WorkLogs { get; set; } = [];
    public virtual ICollection<ClientActItem> ClientActItems { get; set; } = [];
}
