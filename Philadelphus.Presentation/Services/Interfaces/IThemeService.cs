using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Сервис управления темой оформления приложения. Применяет выбранный режим
    /// и сохраняет его в конфигурации (appsettings.json). Конкретное применение темы —
    /// в платформенной реализации (WPF / Avalonia).
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Текущий режим темы.
        /// </summary>
        AppThemeMode CurrentMode { get; }

        /// <summary>
        /// Применить и сохранить режим темы.
        /// </summary>
        /// <param name="mode">Новый режим темы.</param>
        void SetMode(AppThemeMode mode);
    }
}
