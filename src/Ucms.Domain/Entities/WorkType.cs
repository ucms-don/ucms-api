namespace Ucms.Domain.Entities;

using Ucms.Domain.Common;

/// <summary>
/// Ish turi sprvashnigi (e.g. "Стяжка пола", "Штукатурка стен")
/// </summary>
public class WorkType : LocalizableEntity, IDeletable
{
    /// <summary>
    /// Удален или нет
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Ish turi kodi (noyob, ixtiyoriy — bo'sh qoldirilsa avtomatik generatsiya qilinadi)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// O'lchov birligi (ЕИ — справочник). Ölchov birligi reference'iga havola.
    /// </summary>
    public Guid? MeasurementUnitId { get; set; }

    public virtual MeasurementUnit? MeasurementUnit { get; set; }
}
