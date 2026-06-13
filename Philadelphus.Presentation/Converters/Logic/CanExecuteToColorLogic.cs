namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера CanExecuteToColorConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class CanExecuteToColorLogic
    {
        /// <summary>
        /// Возвращает имя цвета: "Green" для доступной команды, "Red" — для недоступной, иначе "Gray".
        /// </summary>
        public static string ResolveColorName(object? value)
            => value is bool canExecute ? (canExecute ? "Green" : "Red") : "Gray";
    }
}
