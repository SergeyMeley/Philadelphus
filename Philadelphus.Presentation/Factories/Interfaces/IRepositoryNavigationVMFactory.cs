using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Interfaces;

/// <summary>
/// Задает контракт фабрики модели навигации по репозиторию.
/// </summary>
public interface IRepositoryNavigationVMFactory
{
    /// <summary>
    /// Создает модель навигации для указанного обозревателя репозитория.
    /// </summary>
    /// <param name="repositoryExplorerVM">Модель обозревателя репозитория.</param>
    /// <returns>Созданная модель навигации.</returns>
    RepositoryNavigationVM Create(RepositoryExplorerControlVM repositoryExplorerVM);
}
