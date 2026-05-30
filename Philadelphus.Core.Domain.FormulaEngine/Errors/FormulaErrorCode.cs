using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.FormulaEngine.Errors
{
    /// <summary>
    /// Коды ошибок формул, отображаемые в результате вычисления.
    /// </summary>
    public enum FormulaErrorCode
    {
        [Display(Name = "#ОШИБКА_ПАРСИНГА", Description = "Ошибка разбора формулы")]
        ParseError,

        [Display(Name = "#НЕИЗВЕСТНАЯ_ФУНКЦИЯ", Description = "Функция не найдена")]
        UnknownFunction,

        [Display(Name = "#НЕВЕРНОЕ_КОЛИЧЕСТВО_АРГУМЕНТОВ", Description = "Количество аргументов не соответствует сигнатуре")]
        InvalidArgumentCount,

        [Display(Name = "#НЕСОВМЕСТИМЫЕ_ТИПЫ", Description = "Тип аргумента или результата не соответствует ожидаемому")]
        TypeMismatch,

        [Display(Name = "#ДЕЛЕНИЕ_НА_НОЛЬ", Description = "Деление на ноль")]
        DivZero,

        [Display(Name = "#ЛИСТ_НЕ_НАЙДЕН", Description = "Лист дерева не найден")]
        TreeLeaveNotFound,

        [Display(Name = "#СВОЙСТВО_НЕ_НАЙДЕНО", Description = "Свойство объекта не найдено")]
        PropertyNotFound,

        [Display(Name = "#АТРИБУТ_НЕ_НАЙДЕН", Description = "Атрибут не найден")]
        AttributeNotFound,

        [Display(Name = "#ДУБЛИКАТ_АТРИБУТА", Description = "Найдено несколько атрибутов с указанным именем")]
        AttributeDuplicate,

        [Display(Name = "#НЕ_РЕАЛИЗОВАНО", Description = "Функциональность зарезервирована, но еще не реализована")]
        NotImplemented,

        [Display(Name = "#ОШИБКА_ЗАГРУЗКИ_ПЛАГИНА", Description = "Ошибка загрузки внешнего поставщика формул")]
        PluginLoadError
    }
}
