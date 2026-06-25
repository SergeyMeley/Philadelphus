using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Factories.Implementations
{
    /// <summary>
    /// Фабрика создания модели представления конструктора импорта из Excel.
    /// </summary>
    public class ExcelImportDesignerVMFactory : IExcelImportDesignerVMFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportDesignerVMFactory" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ExcelImportDesignerVMFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ExcelImportDesignerVM Create(
            ShrubModel shrub,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView)
        {
            ArgumentNullException.ThrowIfNull(shrub);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(refreshRepositoryView);

            var vm = ActivatorUtilities.CreateInstance<ExcelImportDesignerVM>(_serviceProvider);
            vm.Initialize(shrub, repository, repositoryService, refreshRepositoryView);
            return vm;
        }
    }
}
