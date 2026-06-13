namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера IsAvailableToColorConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class IsAvailableToColorLogic
    {
        /// <summary>
        /// Возвращает имя цвета: "Black" для null, "Green"/"Red" для bool, иначе "DarkRed".
        /// </summary>
        public static string ResolveColorName(object? value)
        {
            if (value is null)
                return "Black";

            if (value is bool isAvailable)
                return isAvailable ? "Green" : "Red";

            return "DarkRed";
        }
    }
}
