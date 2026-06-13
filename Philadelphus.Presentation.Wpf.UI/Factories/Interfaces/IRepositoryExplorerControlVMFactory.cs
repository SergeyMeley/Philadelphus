using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с репозитория.
    /// </summary>
    internal interface IRepositoryExplorerControlVMFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryVM">Параметр repositoryVM.</param>
        /// <returns>Созданный объект.</returns>
        public RepositoryExplorerControlVM Create(PhiladelphusRepositoryVM repositoryVM);
    }
}
