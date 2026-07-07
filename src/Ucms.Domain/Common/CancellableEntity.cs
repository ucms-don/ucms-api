namespace Ucms.Domain.Common;

/// <summary>
/// Bekor qilish imkoniga ega bo'lgan auditlanadigan entity uchun asosiy klass.
/// Базовый класс для аудируемых сущностей с поддержкой отмены.
/// </summary>
public abstract class CancellableEntity : AuditableEntity, ICancellable
{
    /// <summary>Bekor qilinganmi?</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Bekor qilingan vaqt</summary>
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary>Bekor qilgan foydalanuvchi ID</summary>
    public Guid? CancelledBy { get; set; }
}
