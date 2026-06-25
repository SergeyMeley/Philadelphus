using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера BooleanToColorConverter (bool/null → цвет, без привязки к UI-фреймворку).
    /// </summary>
    public static class BooleanToColorLogic
    {
        /// <summary>
        /// Возвращает цвет: Green для true, Red для false, Black для null,
        /// DarkRed — для значения, не являющегося bool.
        /// </summary>
        public static ConverterColor ResolveColor(object? value)
        {
            if (value is null)
                return ConverterColor.Black;

            if (value is bool flag)
                return flag ? ConverterColor.Green : ConverterColor.Red;

            return ConverterColor.DarkRed;
        }
    }
}
