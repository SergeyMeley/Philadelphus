namespace Philadelphus.Presentation.Enums
{
    /// <summary>
    /// Палитра именованных цветов, используемых конвертерами (без привязки к UI-фреймворку).
    /// Материализация в конкретную кисть — на стороне платформы.
    /// </summary>
    public enum ConverterColor
    {
        Black,
        White,
        Green,
        Red,
        DarkRed,
        IndianRed,
        OrangeRed,
        DeepPink,
        Cyan,
        YellowGreen,
        StateInitialized,
        StateChanged,
        StateSavedOrLoaded,
        StateForSoftDelete,
        StateForHardDelete,
        StateSoftDeleted
    }
}
