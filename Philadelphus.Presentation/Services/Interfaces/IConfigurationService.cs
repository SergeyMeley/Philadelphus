using Microsoft.Extensions.Options;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.SettingsContainersVMs;
using System.IO;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с IConfigurationService.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Выполняет операцию MoveConfigFile.
        /// </summary>
        /// <param name="configFile">Параметр configFile.</param>
        /// <param name="newDirectory">Параметр newDirectory.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool MoveConfigFile(FileInfo configFile, DirectoryInfo newDirectory);
        
        /// <summary>
        /// Выполняет операцию SelectAnotherConfigFile.
        /// </summary>
        /// <param name="configurationFileVM">Параметр configurationFileVM.</param>
        /// <param name="newFile">Параметр newFile.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool SelectAnotherConfigFile(ConfigurationFileVM configurationFileVM, FileInfo newFile);
        public bool UpdateConfigFile<T>(FileInfo configFile, IOptions<T> newConfigObject) where T : class;
    }
}
