using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Implementations;

/// <summary>
/// Создает модели дерева связей с использованием контейнера зависимостей.
/// </summary>
public sealed class RepositoryRelationsControlVMFactory : IRepositoryRelationsControlVMFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Инициализирует фабрику модели дерева связей.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
    public RepositoryRelationsControlVMFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Создает модель дерева связей для указанного обозревателя репозитория.
    /// </summary>
    /// <param name="repositoryExplorerVM">Модель обозревателя репозитория.</param>
    /// <param name="navigationVM">Модель навигации по репозиторию.</param>
    /// <returns>Созданная модель дерева связей.</returns>
    public RepositoryRelationsControlVM Create(RepositoryExplorerControlVM repositoryExplorerVM,
        RepositoryNavigationVM navigationVM)
    {
        ArgumentNullException.ThrowIfNull(repositoryExplorerVM);
        ArgumentNullException.ThrowIfNull(navigationVM);
        return ActivatorUtilities.CreateInstance<RepositoryRelationsControlVM>(
            _serviceProvider, repositoryExplorerVM, navigationVM);
    }
}
