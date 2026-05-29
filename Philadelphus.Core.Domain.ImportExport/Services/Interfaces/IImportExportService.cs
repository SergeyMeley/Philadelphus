using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Services.Interfaces
{
    /// <summary>
    /// Контракт сервиса импорта и экспорта, скрывающего адаптеры и DTO от вызывающего слоя.
    /// </summary>
    public interface IImportExportService
    {
        /// <summary>
        /// Возвращает поддерживаемые форматы файлов.
        /// </summary>
        /// <returns>Коллекция расширений поддерживаемых файлов.</returns>
        IReadOnlyCollection<string> GetSupportedFileFormats();

        /// <summary>
        /// Сериализует рабочее дерево в файл, выбирая адаптер по формату и наименованию, указанным пользователем.
        /// </summary>
        /// <param name="fileFormat">Формат файла, выбранный пользователем.</param>
        /// <param name="adapterName">Наименование адаптера, выбранного пользователем.</param>
        /// <param name="workingTree">Рабочее дерево.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        void ExportToFile(string fileFormat, string adapterName, WorkingTreeModel workingTree, string filePath);

        /// <summary>
        /// Импортирует рабочее дерево из файла, выбирая адаптер по формату и наименованию, указанным пользователем.
        /// </summary>
        /// <param name="fileFormat">Формат файла, выбранный пользователем.</param>
        /// <param name="adapterName">Наименование адаптера, выбранного пользователем.</param>
        /// <param name="filePath">Путь к исходному файлу.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        /// <returns>Импортированное рабочее дерево.</returns>
        WorkingTreeModel ImportFromFile(
            string fileFormat,
            string adapterName,
            string filePath,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress);

        /// <summary>
        /// Импортирует рабочее дерево из подготовленных данных импорта.
        /// </summary>
        /// <param name="importData">Подготовленные данные импорта.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        /// <returns>Импортированное рабочее дерево.</returns>
        WorkingTreeModel ImportPreparedData(
            WorkingTreeExportDTO importData,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress);
    }
}
