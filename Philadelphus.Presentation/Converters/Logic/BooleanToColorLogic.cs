namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертеров CanExecuteToColorConverter и IsAvailableToColorConverter
    /// (без привязки к UI-фреймворку).
    /// </summary>
    public static class BooleanToColorLogic
    {
        /// <summary>
        /// Возвращает имя цвета: "Green" для true, "Red" для false, "Black" для null,
        /// "DarkRed" — для значения, не являющегося bool.
        /// </summary>
        public static string ResolveColorName(object? value)
        {
            if (value is null)
                return "Black";

            if (value is bool flag)
                return flag ? "Green" : "Red";

            return "DarkRed";
        }
    }
}
