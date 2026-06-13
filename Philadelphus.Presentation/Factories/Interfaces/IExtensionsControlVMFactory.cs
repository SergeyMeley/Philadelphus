using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с расширения.
    /// </summary>
    public interface IExtensionsControlVMFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <returns>Созданный объект.</returns>
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
