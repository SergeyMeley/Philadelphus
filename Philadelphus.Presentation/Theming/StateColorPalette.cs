using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Theming
{
    /// <summary>
    /// Семантическая палитра состояний для светлой и тёмной тем.
    /// </summary>
    public static class StateColorPalette
    {
        /// <summary>
        /// Возвращает HEX-цвет семантической роли состояния.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Переданная роль не является цветом состояния.
        /// </exception>
        public static string ResolveHex(ConverterColor color, bool isDarkTheme)
            => (color, isDarkTheme) switch
            {
                (ConverterColor.StateInitialized, false) => "#1349EC",
                (ConverterColor.StateChanged, false) => "#B82EB8",
                (ConverterColor.StateSavedOrLoaded, false) => "#169C59",
                (ConverterColor.StateForSoftDelete, false) => "#D65F00",
                (ConverterColor.StateForHardDelete, false) => "#8E0B16",
                (ConverterColor.StateSoftDeleted, false) => "#595959",
                (ConverterColor.StateInitialized, true) => "#5B7CFA",
                (ConverterColor.StateChanged, true) => "#E879D2",
                (ConverterColor.StateSavedOrLoaded, true) => "#5EED9A",
                (ConverterColor.StateForSoftDelete, true) => "#FF9F43",
                (ConverterColor.StateForHardDelete, true) => "#D61F2C",
                (ConverterColor.StateSoftDeleted, true) => "#A3A3A3",
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Цвет не является семантической ролью состояния."),
            };
    }
}
