namespace Ucms.Domain.Common;

/// <summary>
/// To'lovni/operatsiyani bekor qilish imkonini beruvchi interfeys.
/// Интерфейс для отменяемых финансовых операций.
/// </summary>
public interface ICancellable
{
    /// <summary>Bekor qilinganmi?</summary>
    bool IsCancelled { get; set; }

    /// <summary>Bekor qilingan vaqt</summary>
    DateTimeOffset? CancelledAt { get; set; }

    /// <summary>Bekor qilgan foydalanuvchi ID</summary>
    Guid? CancelledBy { get; set; }
}
