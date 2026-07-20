namespace Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;

/// <summary>
/// Результат поиска листов по значениям атрибутов.
/// </summary>
public enum LeaveAttributeMatchStatus
{
    /// <summary>
    /// Сравнение невозможно из-за ошибок значений.
    /// </summary>
    Invalid,

    /// <summary>
    /// Подходящие листы отсутствуют.
    /// </summary>
    NotFound,

    /// <summary>
    /// Найден единственный подходящий лист.
    /// </summary>
    Resolved,

    /// <summary>
    /// Найдено несколько подходящих листов.
    /// </summary>
    Ambiguous
}
