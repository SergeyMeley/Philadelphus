using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task LoadExtensionsAsync(IEnumerable<string> pluginsFolderPathes);

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
        Task<TreeRepositoryMemberBaseModel> ExecuteExtensionAsync(ExtensionInstance extension, TreeRepositoryMemberBaseModel element);

        /// <summary>
        /// Запустить все расширения с AutoStart = true
        /// </summary>
        Task AutoStartExtensionsAsync();

        /// <summary>
        /// Получить расширения, совместимые с элементом
        /// </summary>
        Task<List<ExtensionInstance>> GetCompatibleExtensionsAsync(TreeRepositoryMemberBaseModel element);

        event EventHandler<ExtensionLoadedEventArgs> ExtensionLoaded;
        event EventHandler<ExtensionErrorEventArgs> ExtensionError;
    }

}
