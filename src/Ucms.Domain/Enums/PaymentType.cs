namespace Ucms.Domain.Enums;

/// <summary>
/// Тип оплаты
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Бюджетные средства
    /// </summary>
    Budget = 0,

    /// <summary>
    /// Гуманитарная помощь
    /// </summary>
    Humanitarian = 10,

    /// <summary>
    /// Спонсорские фонды
    /// </summary>
    Funds = 20,

    /// <summary>
    /// Грант
    /// </summary>
    Grant = 30,

    /// <summary>
    /// Возврат
    /// </summary>
    Return = 40,
}
