using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    /// <summary>
    /// Фабрика создания репозитория.
    /// </summary>
    internal class RepositoryExplorerControlVMFactory : IRepositoryExplorerControlVMFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerControlVMFactory" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RepositoryExplorerControlVMFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryVM">Параметр repositoryVM.</param>
        /// <returns>Созданный объект.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RepositoryExplorerControlVM Create(PhiladelphusRepositoryVM repositoryVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryVM);

            return ActivatorUtilities.CreateInstance<RepositoryExplorerControlVM>(_serviceProvider, repositoryVM);
        }
    }
}
