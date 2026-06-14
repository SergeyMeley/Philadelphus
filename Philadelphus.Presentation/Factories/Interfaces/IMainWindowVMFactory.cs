using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с главного окна.
    /// </summary>
    public interface IMainWindowVMFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <returns>Созданный объект.</returns>
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
