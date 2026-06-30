namespace Ucms.Domain.Common;

using Ucms.Domain.Enums;

/// <summary>
/// Базовый класс для людей
/// </summary>
public abstract class PersonBase : Entity
{
    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Отчество
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Дата рождение
    /// </summary>
    public DateTimeOffset? BirthDate { get; set; }
}
