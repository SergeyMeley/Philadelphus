using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Координирует проверку и выполнение импорта данных из Excel без привязки к UI.
    /// </summary>
    public class ExcelImportPipeline
    {
        private readonly ExcelImportExportAdapter _excelImportExportAdapter;
        private readonly IExcelImportProfileValidator _profileValidator;
        private readonly IImportExportService _importExportService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportPipeline" />.
        /// </summary>
        /// <param name="excelImportExportAdapter">Адаптер импорта данных из Excel.</param>
        /// <param name="profileValidator">Валидатор профилей импорта Excel.</param>
        /// <param name="importExportService">Сервис импорта-экспорта.</param>
        public ExcelImportPipeline(
            ExcelImportExportAdapter excelImportExportAdapter,
            IExcelImportProfileValidator profileValidator,
            IImportExportService importExportService)
        {
            ArgumentNullException.ThrowIfNull(excelImportExportAdapter);
            ArgumentNullException.ThrowIfNull(profileValidator);
            ArgumentNullException.ThrowIfNull(importExportService);

            _excelImportExportAdapter = excelImportExportAdapter;
            _profileValidator = profileValidator;
            _importExportService = importExportService;
        }

        /// <summary>
        /// Возвращает профили Excel, включенные в выполнение импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Коллекция профилей, включенных в импорт.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution(ExcelImportSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            return ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
        }

        /// <summary>
        /// Проверяет схему импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Результат проверки схемы.</returns>
        public ExcelImportValidationResult Validate(ExcelImportSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

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
            var importResult = BuildImportData(schema);

            return JsonSerializer.Serialize(importResult, CreateJsonOptions());
        }

        /// <summary>
        /// Формирует DTO рабочего дерева по схеме импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public WorkingTreeExportDTO BuildImportData(ExcelImportSchema schema)
        {
            EnsureValid(schema);
            return _excelImportExportAdapter.Parse(ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema));
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
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);

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
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentException.ThrowIfNullOrWhiteSpace(emptySelectionMessage);

            var profiles = ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
            if (profiles.Count == 0)
            {
                throw new InvalidOperationException(emptySelectionMessage);
            }
        }

        /// <summary>
        /// Импортирует подготовленные DTO-данные в репозиторий.
        /// </summary>
        /// <param name="importData">Подготовленные данные импорта.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <returns>Импортированное рабочее дерево.</returns>
        public WorkingTreeModel ImportPreparedData(
            WorkingTreeExportDTO importData,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService)
        {
            ArgumentNullException.ThrowIfNull(importData);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);

            return _importExportService.ImportPreparedData(
                importData,
                repository,
                repositoryService,
                _ => { },
                (_, _) => { });
        }

        private void EnsureValid(ExcelImportSchema schema)
        {
            var validationResult = Validate(schema);
            if (validationResult.HasErrors)
            {
                throw new InvalidOperationException(ExcelImportValidationMessageBuilder.Build(validationResult));
            }
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
