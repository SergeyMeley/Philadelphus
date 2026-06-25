namespace Philadelphus.Presentation.Configurations
{
    /// <summary>
    /// Настройки оформления приложения (раздел appsettings.json).
    /// Относится к слою представления, а не к доменному слою.
    /// </summary>
    public class AppearanceConfig
    {
        /// <summary>
        /// Сохранённое значение темы оформления (нейтральная строка: System / Light / Dark).
        /// Разбор в конкретный режим выполняется в слое представления.
        /// </summary>
        public string? ThemeString { get; set; }
    }
}
