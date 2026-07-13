using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Factories.Implementations;

/// <summary>
/// Создает модели навигации по репозиторию с использованием контейнера зависимостей.
/// </summary>
public sealed class RepositoryNavigationVMFactory : IRepositoryNavigationVMFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Инициализирует фабрику модели навигации.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
    public RepositoryNavigationVMFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Создает модель навигации для указанного обозревателя репозитория.
    /// </summary>
    /// <param name="repositoryExplorerVM">Модель обозревателя репозитория.</param>
    /// <returns>Созданная модель навигации.</returns>
    public RepositoryNavigationVM Create(RepositoryExplorerControlVM repositoryExplorerVM)
    {
        ArgumentNullException.ThrowIfNull(repositoryExplorerVM);
        return ActivatorUtilities.CreateInstance<RepositoryNavigationVM>(_serviceProvider, repositoryExplorerVM);
    }
}
