using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.FormulaEngine.Errors
{
    /// <summary>
    /// Коды ошибок формул, отображаемые в результате вычисления.
    /// </summary>
    public enum FormulaErrorCode
    {
        /// <summary>
        /// Формулу не удалось разобрать.
        /// </summary>
        [Display(Name = "#ОШИБКА_ПАРСИНГА", Description = "Ошибка разбора формулы")]
        ParseError,

        /// <summary>
        /// Функция или оператор не зарегистрированы в реестре формул.
        /// </summary>
        [Display(Name = "#НЕИЗВЕСТНАЯ_ФУНКЦИЯ", Description = "Функция не найдена")]
        UnknownFunction,

        /// <summary>
        /// Количество переданных аргументов не соответствует сигнатуре формулы.
        /// </summary>
        [Display(Name = "#НЕВЕРНОЕ_КОЛИЧЕСТВО_АРГУМЕНТОВ", Description = "Количество аргументов не соответствует сигнатуре")]
        InvalidArgumentCount,

        /// <summary>
        /// Тип аргумента или результата не соответствует ожидаемому типу.
        /// </summary>
        [Display(Name = "#НЕСОВМЕСТИМЫЕ_ТИПЫ", Description = "Тип аргумента или результата не соответствует ожидаемому")]
        TypeMismatch,

        /// <summary>
        /// Значение аргумента не входит в допустимую область значений формулы.
        /// </summary>
        [Display(Name = "#НЕКОРРЕКТНОЕ_ЗНАЧЕНИЕ", Description = "Значение аргумента не входит в допустимую область")]
        InvalidArgumentValue,

        /// <summary>
        /// При вычислении выполнено деление на ноль.
        /// </summary>
        [Display(Name = "#ДЕЛЕНИЕ_НА_НОЛЬ", Description = "Деление на ноль")]
        DivZero,

        /// <summary>
        /// Ссылка указывает на отсутствующий лист дерева.
        /// </summary>
        [Display(Name = "#ЛИСТ_НЕ_НАЙДЕН", Description = "Лист дерева не найден")]
        TreeLeaveNotFound,

        /// <summary>
        /// Запрошенное свойство объекта не найдено.
        /// </summary>
        [Display(Name = "#СВОЙСТВО_НЕ_НАЙДЕНО", Description = "Свойство объекта не найдено")]
        PropertyNotFound,

        /// <summary>
        /// Запрошенный атрибут объекта не найден.
        /// </summary>
        [Display(Name = "#АТРИБУТ_НЕ_НАЙДЕН", Description = "Атрибут не найден")]
        AttributeNotFound,

        /// <summary>
        /// По запрошенному имени найдено несколько атрибутов.
        /// </summary>
        [Display(Name = "#ДУБЛИКАТ_АТРИБУТА", Description = "Найдено несколько атрибутов с указанным именем")]
        AttributeDuplicate,

        /// <summary>
        /// Функциональность зарезервирована, но еще не реализована.
        /// </summary>
        [Display(Name = "#НЕ_РЕАЛИЗОВАНО", Description = "Функциональность зарезервирована, но еще не реализована")]
        NotImplemented,

        /// <summary>
        /// Внешний поставщик формул не удалось загрузить.
        /// </summary>
        [Display(Name = "#ОШИБКА_ЗАГРУЗКИ_ПЛАГИНА", Description = "Ошибка загрузки внешнего поставщика формул")]
        PluginLoadError
    }
}
