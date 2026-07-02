namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера SelectedIndexConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class SelectedIndexLogic
    {
        /// <summary>
        /// Возвращает 1-based номер (index + 1) в виде строки.
        /// </summary>
        public static string Convert(object? value)
            => value is null ? string.Empty : ((int)value + 1).ToString();
    }
}
