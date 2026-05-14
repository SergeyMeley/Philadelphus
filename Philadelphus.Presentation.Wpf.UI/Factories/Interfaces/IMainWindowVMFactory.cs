using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с главного окна.
    /// </summary>
    internal interface IMainWindowVMFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <returns>Созданный объект.</returns>
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM);
    }
}
