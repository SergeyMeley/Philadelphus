using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Factories.Interfaces
{
    /// <summary>
    /// Задает контракт для работы с репозитория.
    /// </summary>
    public interface IRepositoryExplorerControlVMFactory
    {
        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryVM">Параметр repositoryVM.</param>
        /// <returns>Созданный объект.</returns>
        public RepositoryExplorerControlVM Create(PhiladelphusRepositoryVM repositoryVM);
    }
}
