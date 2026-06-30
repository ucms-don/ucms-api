namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип покрытия — smeta pozitsiyasi qaysi yuzaga tegishli (devor / pol / shift).
/// </summary>
public enum SurfaceType
{
    /// <summary>
    /// Стены (devorlar)
    /// </summary>
    [Display(Name = "SurfaceType_Walls")]
    Walls = 1,

    /// <summary>
    /// Полы (pollar)
    /// </summary>
    [Display(Name = "SurfaceType_Floors")]
    Floors = 2,

    /// <summary>
    /// Потолки (shiftlar)
    /// </summary>
    [Display(Name = "SurfaceType_Ceilings")]
    Ceilings = 3,
}
