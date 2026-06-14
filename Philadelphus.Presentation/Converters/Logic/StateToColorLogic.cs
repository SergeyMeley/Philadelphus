using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера StateToColorConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class StateToColorLogic
    {
        /// <summary>
        /// Возвращает цвет для состояния сущности; White по умолчанию.
        /// </summary>
        public static ConverterColor ResolveColor(object? value)
            => value is State state
                ? state switch
                {
                    State.Initialized => ConverterColor.DeepPink,
                    State.Changed => ConverterColor.Cyan,
                    State.SavedOrLoaded => ConverterColor.YellowGreen,
                    State.ForSoftDelete => ConverterColor.OrangeRed,
                    State.ForHardDelete => ConverterColor.Red,
                    State.SoftDeleted => ConverterColor.IndianRed,
                    _ => ConverterColor.White
                }
                : ConverterColor.White;
    }
}
