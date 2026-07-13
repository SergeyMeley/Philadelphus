using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Interfaces;

/// <summary>
/// Задает контракт фабрики модели дерева связей репозитория.
/// </summary>
public interface IRepositoryRelationsControlVMFactory
{
    /// <summary>
    /// Создает модель дерева связей для указанного обозревателя репозитория.
    /// </summary>
    /// <param name="repositoryExplorerVM">Модель обозревателя репозитория.</param>
    /// <param name="navigationVM">Модель навигации по репозиторию.</param>
    /// <returns>Созданная модель дерева связей.</returns>
    RepositoryRelationsControlVM Create(RepositoryExplorerControlVM repositoryExplorerVM,
        RepositoryNavigationVM navigationVM);
}
