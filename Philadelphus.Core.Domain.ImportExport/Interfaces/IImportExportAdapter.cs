using Philadelphus.Core.Domain.ImportExport.Entities.DTOs.ImportExportDTOs;

namespace Philadelphus.Core.Domain.ImportExport.Interfaces
{
    /// <summary>
    /// Контракт адаптера импорта и экспорта для одного поддерживаемого формата файла.
    /// </summary>
    public interface IImportExportAdapter
    {
        /// <summary>
        /// Формат файла, с которым работает адаптер.
        /// </summary>
        string FileFormat { get; }

        /// <summary>
        /// Наименование адаптера.
        /// </summary>
        string AdapterName { get; }

        /// <summary>
        /// Сериализует DTO импорта-экспорта в файл поддерживаемого формата.
        /// </summary>
        /// <param name="dto">DTO рабочего дерева.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        void Serialize(WorkingTreeExportDTO dto, string filePath);

        /// <summary>
        /// Читает файл поддерживаемого формата и преобразует его в DTO импорта-экспорта.
        /// </summary>
        /// <param name="filePath">Путь к исходному файлу.</param>
        /// <returns>DTO рабочего дерева.</returns>
        WorkingTreeExportDTO Parse(string filePath);
    }
}
