using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Factories.Interfaces
{
    /// <summary>
    /// Задаёт контракт создания модели представления конструктора импорта из Excel.
    /// </summary>
    public interface IExcelImportDesignerVMFactory
    {
        /// <summary>
        /// Создаёт модель представления конструктора импорта с рантайм-контекстом.
        /// </summary>
        /// <param name="shrub">Рабочее дерево активного репозитория.</param>
        /// <param name="repository">Активный репозиторий.</param>
        /// <param name="repositoryService">Сервис работы с репозиторием.</param>
        /// <param name="refreshRepositoryView">Колбэк обновления представления репозитория.</param>
        /// <returns>Созданная модель представления.</returns>
        public ExcelImportDesignerVM Create(
            ShrubModel shrub,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView);
    }
}
