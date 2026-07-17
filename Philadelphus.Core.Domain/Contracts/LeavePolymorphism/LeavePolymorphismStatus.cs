namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Результат вычисления полиморфного родителя листа.
/// </summary>
public enum LeavePolymorphismStatus
{
    /// <summary>Сравнение невозможно из-за структуры или ошибок значений.</summary>
    Invalid,

    /// <summary>Подходящие родительские листы отсутствуют.</summary>
    NotFound,

    /// <summary>Найдено несколько подходящих родительских листов.</summary>
    Ambiguous,

    /// <summary>Найден единственный подходящий родительский лист.</summary>
    Resolved
}
