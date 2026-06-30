namespace Ucms.Domain.Common;

/// <summary>
/// Базовый класс для сущностей поддерживающих аудит 
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    /// <summary>
    /// Запись создан в 
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Запись обновлен в
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Запис создан кем
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Запис обновлен кем
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
