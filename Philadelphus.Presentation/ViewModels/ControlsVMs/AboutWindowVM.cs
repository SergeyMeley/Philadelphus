namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления окна «О программе».
    /// </summary>
    public class AboutWindowVM : ViewModelBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AboutWindowVM" />.
        /// </summary>
        /// <param name="version">Строка версии приложения.</param>
        public AboutWindowVM(string version)
        {
            Version = version ?? string.Empty;
        }

        /// <summary>
        /// Название приложения.
        /// </summary>
        public string AppName => "Чубушник";

        /// <summary>
        /// Версия приложения.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Информация об использованных ресурсах.
        /// </summary>
        public string Credit => "Эта обложка была разработана с использованием ресурсов сайта Flaticon.com";
    }
}
