using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Services.Implementations
{
    /// <summary>
    /// Сервис импорта и экспорта, выбирающий адаптер по явно указанному формату файла и наименованию адаптера.
    /// </summary>
    public class ImportExportService : IImportExportService
    {
        private readonly Dictionary<ImportExportAdapterKey, IImportExportAdapter> _adaptersByKey;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportService" />.
        /// </summary>
        /// <param name="adapters">Зарегистрированные адаптеры импорта и экспорта.</param>
        public ImportExportService(IEnumerable<IImportExportAdapter> adapters)
        {
            ArgumentNullException.ThrowIfNull(adapters);

            _adaptersByKey = BuildAdaptersMap(adapters);
        }

        /// <summary>
        /// Возвращает поддерживаемые форматы файлов.
        /// </summary>
        /// <returns>Коллекция расширений поддерживаемых файлов.</returns>
        public IReadOnlyCollection<string> GetSupportedFileFormats()
        {
            return _adaptersByKey.Keys
                .Select(key => key.FileFormat)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Сериализует рабочее дерево в файл, выбирая адаптер по формату и наименованию, указанным пользователем.
        /// </summary>
        /// <param name="fileFormat">Формат файла, выбранный пользователем.</param>
        /// <param name="adapterName">Наименование адаптера, выбранного пользователем.</param>
        /// <param name="workingTree">Рабочее дерево.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        public void Serialize(string fileFormat, string adapterName, WorkingTreeModel workingTree, string filePath)
        {
            ArgumentNullException.ThrowIfNull(workingTree);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var dto = new WorkingTreeExportDTO(workingTree);
            GetAdapter(fileFormat, adapterName).Serialize(dto, filePath);
        }

        private IImportExportAdapter GetAdapter(string fileFormat, string adapterName)
        {
            var key = ImportExportAdapterKey.Create(fileFormat, adapterName);
            if (_adaptersByKey.TryGetValue(key, out var adapter))
            {
                return adapter;
            }

            throw new InvalidOperationException(
                $"Для формата '{key.FileFormat}' и адаптера '{adapterName}' не зарегистрирован адаптер импорта-экспорта.");
        }

        private static Dictionary<ImportExportAdapterKey, IImportExportAdapter> BuildAdaptersMap(IEnumerable<IImportExportAdapter> adapters)
        {
            var result = new Dictionary<ImportExportAdapterKey, IImportExportAdapter>();

            foreach (var adapter in adapters)
            {
                ArgumentNullException.ThrowIfNull(adapter);

                var key = ImportExportAdapterKey.Create(adapter.FileFormat, adapter.AdapterName);
                if (result.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        $"Для формата '{key.FileFormat}' и адаптера '{adapter.AdapterName}' зарегистрировано несколько адаптеров импорта-экспорта.");
                }

                result.Add(key, adapter);
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Не зарегистрированы адаптеры импорта-экспорта.");
            }

            return result;
        }

        private readonly record struct ImportExportAdapterKey(string FileFormat, string AdapterName)
        {
            /// <summary>
            /// Создает нормализованный ключ адаптера импорта-экспорта.
            /// </summary>
            /// <param name="fileFormat">Формат файла.</param>
            /// <param name="adapterName">Наименование адаптера.</param>
            /// <returns>Ключ адаптера импорта-экспорта.</returns>
            public static ImportExportAdapterKey Create(string fileFormat, string adapterName)
            {
                return new ImportExportAdapterKey(
                    NormalizeFormat(fileFormat),
                    NormalizeAdapterName(adapterName));
            }
        }

        private static string NormalizeFormat(string fileFormat)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);

            var format = fileFormat.Trim().ToLowerInvariant();
            if (format.StartsWith(".", StringComparison.Ordinal) == false)
            {
                format = $".{format}";
            }

            return format;
        }

        private static string NormalizeAdapterName(string adapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            return adapterName.Trim().ToLowerInvariant();
        }
    }
}
