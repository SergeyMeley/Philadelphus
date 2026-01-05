using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;

namespace Philadelphus.Core.Domain.ExtensionSystem.Services
{
    /// <summary>
    /// Интерфейс менеджера расширений
    /// </summary>
    public interface IExtensionManager
    {
        /// <summary>
        /// Получить все загруженные расширения
        /// </summary>
        IReadOnlyList<ExtensionInstance> GetExtensions();

        /// <summary>
        /// Загрузить все расширения
        /// </summary>
        Task LoadExtensionsAsync(string pluginsFolderPath);

        /// <summary>
        /// Запустить расширение
        /// </summary>
        Task StartExtensionAsync(ExtensionInstance extension);

        /// <summary>
        /// Остановить расширение
        /// </summary>
        Task StopExtensionAsync(ExtensionInstance extension);

        /// <summary>
        /// Выполнить основной метод расширения
        /// </summary>
        Task<MainEntityBaseModel> ExecuteExtensionAsync(ExtensionInstance extension, MainEntityBaseModel element);

        /// <summary>
        /// Запустить все расширения с AutoStart = true
        /// </summary>
        Task AutoStartExtensionsAsync();

        /// <summary>
        /// Получить расширения, совместимые с элементом
        /// </summary>
        Task<List<ExtensionInstance>> GetCompatibleExtensionsAsync(MainEntityBaseModel element);

        event EventHandler<ExtensionLoadedEventArgs> ExtensionLoaded;
        event EventHandler<ExtensionErrorEventArgs> ExtensionError;
    }

}
