using AutoMapper;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Entities;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.ImportExport.Mapping;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Services.Implementations
{
    /// <summary>
    /// Сервис импорта и экспорта, выбирающий адаптер по явно указанному формату файла и наименованию адаптера.
    /// </summary>
    public class ImportExportService : IImportExportService
    {
        private readonly Dictionary<ImportExportAdapterKey, IImportExportAdapter> _adaptersByKey;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportService" />.
        /// </summary>
        /// <param name="adapters">Зарегистрированные адаптеры импорта и экспорта.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        public ImportExportService(
            IEnumerable<IImportExportAdapter> adapters, 
            IMapper mapper)
        {
            ArgumentNullException.ThrowIfNull(adapters);
            ArgumentNullException.ThrowIfNull(mapper);

            _adaptersByKey = BuildAdaptersMap(adapters);
            _mapper = mapper;
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
        /// Возвращает доступные адаптеры импорта-экспорта.
        /// </summary>
        /// <returns>Коллекция описаний доступных адаптеров.</returns>
        public IReadOnlyCollection<ImportExportAdapterInfo> GetAvailableAdapters()
        {
            return _adaptersByKey
                .OrderBy(pair => pair.Key.FileFormat)
                .ThenBy(pair => pair.Value.AdapterName)
                .Select(pair => new ImportExportAdapterInfo(
                    pair.Value.FileFormat,
                    pair.Value.AdapterName,
                    pair.Value.CanImport,
                    pair.Value.CanExport))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Экспортирует рабочее дерево в файл, выбирая адаптер по формату и наименованию, указанным пользователем.
        /// </summary>
        /// <param name="fileFormat">Формат файла, выбранный пользователем.</param>
        /// <param name="adapterName">Наименование адаптера, выбранного пользователем.</param>
        /// <param name="workingTree">Рабочее дерево.</param>
        /// <param name="filePath">Путь к файлу результата.</param>
        public void ExportToFile(string fileFormat, string adapterName, WorkingTreeModel workingTree, string filePath)
        {
            ArgumentNullException.ThrowIfNull(workingTree);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var dto = _mapper.Map<WorkingTreeExportDTO>(workingTree);
            GetAdapter(fileFormat, adapterName).Serialize(dto, filePath);
        }

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
        public WorkingTreeModel ImportFromFile(
            string fileFormat,
            string adapterName,
            string filePath,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(refreshProcess);
            ArgumentNullException.ThrowIfNull(refreshProgress);

            var dto = GetAdapter(fileFormat, adapterName).Parse(filePath);
            return ImportPreparedData(dto, repository, repositoryService, refreshProcess, refreshProgress);
        }

        /// <summary>
        /// Конвертирует файл между форматами доступных адаптеров.
        /// </summary>
        /// <param name="sourceFileFormat">Формат исходного файла.</param>
        /// <param name="sourceAdapterName">Наименование адаптера чтения исходного файла.</param>
        /// <param name="sourceFilePath">Путь к исходному файлу.</param>
        /// <param name="targetFileFormat">Формат результирующего файла.</param>
        /// <param name="targetAdapterName">Наименование адаптера записи результирующего файла.</param>
        /// <param name="targetFilePath">Путь к результирующему файлу.</param>
        public void ConvertFile(
            string sourceFileFormat,
            string sourceAdapterName,
            string sourceFilePath,
            string targetFileFormat,
            string targetAdapterName,
            string targetFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetFilePath);

            // Конвертация проходит через единый DTO, чтобы адаптеры не зависели друг от друга.
            var dto = GetAdapter(sourceFileFormat, sourceAdapterName).Parse(sourceFilePath);
            GetAdapter(targetFileFormat, targetAdapterName).Serialize(dto, targetFilePath);
        }

        /// <summary>
        /// Импортирует рабочее дерево из подготовленных данных импорта.
        /// </summary>
        /// <param name="importData">Подготовленные данные импорта.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        /// <returns>Импортированное рабочее дерево.</returns>
        public WorkingTreeModel ImportPreparedData(
            WorkingTreeExportDTO importData,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress)
        {
            ArgumentNullException.ThrowIfNull(importData);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(refreshProcess);
            ArgumentNullException.ThrowIfNull(refreshProgress);

            return _mapper.Map<WorkingTreeModel>(importData, options =>
            {
                options.Items.SetImportRepository(repository);
                options.Items.SetImportRepositoryService(repositoryService);
                options.Items.SetImportRefreshProcess(refreshProcess);
                options.Items.SetImportRefreshProgress(refreshProgress);
            });
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
