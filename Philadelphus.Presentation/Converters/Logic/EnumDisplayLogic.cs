using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера значений DisplayAttribute для enum (без привязки к UI-фреймворку).
    /// </summary>
    public static class EnumDisplayLogic
    {
        /// <summary>
        /// Возвращает отображаемое имя или описание (parameter == "Description") enum-значения.
        /// </summary>
        public static string? Convert(object? value, object? parameter)
        {
            if (value is not Enum enumValue)
            {
                return value?.ToString();
            }

            return parameter?.ToString() == "Description"
                ? enumValue.GetDisplayDescription()
                : enumValue.GetDisplayName();
        }
    }
}
