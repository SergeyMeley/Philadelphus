using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// Координирует проверку, предпросмотр и импорт данных из Excel.
    /// </summary>
    public class ExcelImportPipeline
    {
        private readonly ExcelImportExportAdapter _excelImportExportAdapter;
        private readonly IExcelImportProfileValidator _profileValidator;
        private readonly ExcelImportRepositoryPreviewBuilder _repositoryPreviewBuilder;
        private readonly IImportExportService _importExportService;
        private readonly IPhiladelphusRepositoryCollectionService _repositoryCollectionService;
        private readonly IPhiladelphusRepositoryService _repositoryService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVm;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportPipeline" />.
        /// </summary>
        /// <param name="excelImportExportAdapter">Адаптер импорта данных из Excel.</param>
        /// <param name="profileValidator">Валидатор профилей импорта Excel.</param>
        /// <param name="repositoryPreviewBuilder">Построитель предпросмотра репозитория.</param>
        /// <param name="importExportService">Сервис импорта-экспорта.</param>
        /// <param name="repositoryCollectionService">Сервис коллекции репозиториев.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="dataStoragesCollectionVm">Модель представления хранилищ данных.</param>
        public ExcelImportPipeline(
            ExcelImportExportAdapter excelImportExportAdapter,
            IExcelImportProfileValidator profileValidator,
            ExcelImportRepositoryPreviewBuilder repositoryPreviewBuilder,
            IImportExportService importExportService,
            IPhiladelphusRepositoryCollectionService repositoryCollectionService,
            IPhiladelphusRepositoryService repositoryService,
            DataStoragesCollectionVM dataStoragesCollectionVm)
        {
            _excelImportExportAdapter = excelImportExportAdapter;
            _profileValidator = profileValidator;
            _repositoryPreviewBuilder = repositoryPreviewBuilder;
            _importExportService = importExportService;
            _repositoryCollectionService = repositoryCollectionService;
            _repositoryService = repositoryService;
            _dataStoragesCollectionVm = dataStoragesCollectionVm;
        }

        /// <summary>
        /// Возвращает профили Excel, включенные в выполнение импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Коллекция профилей, включенных в импорт.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution(ExcelImportSchema schema)
        {
            return ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
        }

        /// <summary>
        /// Проверяет схему импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Результат проверки схемы.</returns>
        public ExcelImportValidationResult Validate(ExcelImportSchema schema)
        {
            ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema);
            var profiles = GetProfilesForExecution(schema);
            return _profileValidator.ValidateProfiles(schema.SourceFilePath, profiles);
        }

        /// <summary>
        /// Формирует JSON-представление результата импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>JSON-представление результата импорта Excel.</returns>
        public string BuildJson(ExcelImportSchema schema)
        {
            EnsureValid(schema);
            var importResult = BuildImportData(schema);

            return JsonSerializer.Serialize(importResult, CreateJsonOptions());
        }

        /// <summary>
        /// Строит модель предпросмотра репозитория по схеме импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <param name="targetExistingRootName">Имя существующего корня, в который планируется импорт.</param>
        /// <returns>Модель предпросмотра обозревателя репозитория.</returns>
        public RepositoryExplorerControlVM BuildRepositoryPreview(
            ExcelImportSchema schema,
            string? targetExistingRootName)
        {
            var importData = BuildImportData(schema);
            var previewRepository = CreatePreviewRepository();
            _repositoryService.GetShrubContent(previewRepository);

            var previewTree = ImportPreparedData(importData, previewRepository, _repositoryService);
            return _repositoryPreviewBuilder.Build(previewRepository, previewTree, targetExistingRootName);
        }

        /// <summary>
        /// Формирует краткое описание результата предпросмотра.
        /// </summary>
        /// <param name="previewViewModel">Модель предпросмотра обозревателя репозитория.</param>
        /// <returns>Краткое описание результата предпросмотра.</returns>
        public string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            return _repositoryPreviewBuilder.BuildSummary(previewViewModel);
        }

        /// <summary>
        /// Импортирует данные Excel в репозиторий.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="existingRoot">Существующий корень, в который выполняется импорт.</param>
        public void ImportToRepository(
            ExcelImportSchema schema,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            TreeRootModel? existingRoot)
        {
            var importData = BuildImportData(schema);
            ImportPreparedData(importData, repository, repositoryService);
        }

        /// <summary>
        /// Проверяет, что в схеме есть хотя бы один включенный лист.
        /// </summary>
        /// <param name="schema">Схема импорта.</param>
        /// <param name="emptySelectionMessage">Сообщение об ошибке для пустого набора листов.</param>
        public static void EnsureHasEnabledSheets(ExcelImportSchema schema, string emptySelectionMessage)
        {
            var profiles = ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
            if (profiles.Count == 0)
            {
                throw new InvalidOperationException(emptySelectionMessage);
            }
        }

        private void EnsureValid(ExcelImportSchema schema)
        {
            var validationResult = Validate(schema);
            if (validationResult.HasErrors)
            {
                throw new InvalidOperationException(ExcelImportValidationMessageBuilder.Build(validationResult));
            }
        }

        private WorkingTreeExportDTO BuildImportData(ExcelImportSchema schema)
        {
            EnsureValid(schema);
            return _excelImportExportAdapter.Parse(ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema));
        }

        private WorkingTreeModel ImportPreparedData(
            WorkingTreeExportDTO importData,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService)
        {
            return _importExportService.ImportPreparedData(
                importData,
                repository,
                repositoryService,
                _ => { },
                (_, _) => { });
        }

        private PhiladelphusRepositoryModel CreatePreviewRepository()
        {
            var previewStorage = _dataStoragesCollectionVm.MainDataStorageVM?.Model
                ?? _dataStoragesCollectionVm.DataStoragesVMs?.Select(x => x.Model).FirstOrDefault(x => x != null);

            if (previewStorage == null)
            {
                throw new InvalidOperationException("Не удалось получить временное хранилище для предпросмотра.");
            }

            var previewRepository = _repositoryCollectionService.CreateNewPhiladelphusRepository(previewStorage, needAutoName: false);
            previewRepository.Name = "Предпросмотр импорта";
            previewRepository.Description = "Временный репозиторий для предпросмотра дерева из Excel";
            return previewRepository;
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
