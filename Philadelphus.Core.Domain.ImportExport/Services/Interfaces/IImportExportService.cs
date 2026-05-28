using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;

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
        void Serialize(string fileFormat, string adapterName, WorkingTreeModel workingTree, string filePath);
    }
}
