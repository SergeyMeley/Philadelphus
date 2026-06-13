using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера StateToColorConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class StateToColorLogic
    {
        /// <summary>
        /// Возвращает имя цвета (named color) для состояния сущности; "White" по умолчанию.
        /// </summary>
        public static string ResolveColorName(object? value)
            => value is State state
                ? state switch
                {
                    State.Initialized => "DeepPink",
                    State.Changed => "Cyan",
                    State.SavedOrLoaded => "YellowGreen",
                    State.ForSoftDelete => "OrangeRed",
                    State.ForHardDelete => "Red",
                    State.SoftDeleted => "IndianRed",
                    _ => "White"
                }
                : "White";
    }
}
