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
                    State.Initialized => ConverterColor.StateInitialized,
                    State.Changed => ConverterColor.StateChanged,
                    State.SavedOrLoaded => ConverterColor.StateSavedOrLoaded,
                    State.ForSoftDelete => ConverterColor.StateForSoftDelete,
                    State.ForHardDelete => ConverterColor.StateForHardDelete,
                    State.SoftDeleted => ConverterColor.StateSoftDeleted,
                    _ => ConverterColor.White
                }
                : ConverterColor.White;
    }
}
