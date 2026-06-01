using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Хранит состояние настройки Excel-импорта и выполняет операции, не зависящие от UI.
    /// </summary>
    public class ExcelImportSessionState
    {
        private readonly ExcelPreviewService _previewService;
        private readonly IExcelImportSchemaBuilder _schemaBuilder;
        private readonly ExcelImportPipeline _importPipeline;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportSessionState" />.
        /// </summary>
        /// <param name="previewService">Сервис предпросмотра источников Excel.</param>
        /// <param name="schemaBuilder">Построитель схемы импорта Excel.</param>
        /// <param name="importPipeline">Пайплайн выполнения Excel-импорта.</param>
        public ExcelImportSessionState(
            ExcelPreviewService previewService,
            IExcelImportSchemaBuilder schemaBuilder,
            ExcelImportPipeline importPipeline)
        {
            ArgumentNullException.ThrowIfNull(previewService);
            ArgumentNullException.ThrowIfNull(schemaBuilder);
            ArgumentNullException.ThrowIfNull(importPipeline);

            _previewService = previewService;
            _schemaBuilder = schemaBuilder;
            _importPipeline = importPipeline;
        }

        /// <summary>
        /// Путь к выбранному Excel-файлу.
        /// </summary>
        public string SelectedFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Предпросмотр структуры Excel-книги.
        /// </summary>
        public ExcelPreviewWorkbookInfo? WorkbookPreview { get; private set; }

        /// <summary>
        /// Текущая схема импорта Excel.
        /// </summary>
        public ExcelImportSchema? Schema { get; private set; }

        /// <summary>
        /// Текущий лист схемы импорта.
        /// </summary>
        public ExcelImportSheetSchema? CurrentSheet { get; set; }

        /// <summary>
        /// Загружает Excel-книгу и создает черновую схему импорта.
        /// </summary>
        /// <param name="filePath">Путь к Excel-файлу.</param>
        /// <param name="rootName">Наименование корня по умолчанию.</param>
        public void LoadWorkbook(string filePath, string rootName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(rootName);

            SelectedFilePath = filePath;
            WorkbookPreview = _previewService.GetWorkbookPreview(filePath);
            Schema = _schemaBuilder.CreateDraftSchema(filePath, rootName);
            CurrentSheet = Schema.Sheets.FirstOrDefault();
        }

        /// <summary>
        /// Использует готовую схему импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        public void UseSchema(ExcelImportSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            Schema = schema;
            ExcelImportSchemaNormalizer.EnsureEditableState(Schema);
            ExcelImportSchemaNormalizer.RefreshRelationProjection(Schema);

            SelectedFilePath = Schema.SourceFilePath;
            WorkbookPreview = string.IsNullOrWhiteSpace(SelectedFilePath) || File.Exists(SelectedFilePath) == false
                ? null
                : _previewService.GetWorkbookPreview(SelectedFilePath);
            CurrentSheet = Schema.Sheets.FirstOrDefault();
        }

        /// <summary>
        /// Возвращает предпросмотр данных листа.
        /// </summary>
        /// <param name="sheet">Лист схемы импорта.</param>
        /// <returns>Таблица предпросмотра.</returns>
        public ExcelPreviewTable GetPreview(ExcelImportSheetSchema sheet)
        {
            ArgumentNullException.ThrowIfNull(sheet);

            return _previewService.GetPreview(SelectedFilePath, sheet.Profile.SourceSelection);
        }

        /// <summary>
        /// Возвращает лист схемы по имени источника.
        /// </summary>
        /// <param name="sourceName">Имя источника Excel.</param>
        /// <returns>Лист схемы или null.</returns>
        public ExcelImportSheetSchema? GetSheet(string? sourceName)
        {
            return Schema?.Sheets.FirstOrDefault(x => string.Equals(x.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Включает или отключает все листы схемы.
        /// </summary>
        /// <param name="isEnabled">Признак включения листов.</param>
        public void SetAllSheetsEnabled(bool isEnabled)
        {
            if (Schema == null)
            {
                return;
            }

            foreach (var sheet in Schema.Sheets)
            {
                sheet.IsEnabled = isEnabled;
            }
        }

        /// <summary>
        /// Включает или отключает лист схемы.
        /// </summary>
        /// <param name="sourceName">Имя источника Excel.</param>
        /// <param name="isEnabled">Признак включения листа.</param>
        public void SetSheetEnabled(string sourceName, bool isEnabled)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

            var sheet = GetSheet(sourceName);
            if (sheet != null)
            {
                sheet.IsEnabled = isEnabled;
            }
        }

        /// <summary>
        /// Синхронизирует настройки корня со схемой импорта.
        /// </summary>
        /// <param name="createNewRoot">Признак создания нового корня.</param>
        /// <param name="rootName">Наименование корня.</param>
        public void SyncRootSettings(bool createNewRoot, string rootName)
        {
            if (Schema == null)
            {
                return;
            }

            Schema.SourceFilePath = SelectedFilePath;
            Schema.CreateNewRoot = createNewRoot;
            Schema.RootName = rootName;
            ExcelImportSchemaNormalizer.RefreshRelationProjection(Schema);
        }

        /// <summary>
        /// Возвращает профили, включенные в импорт.
        /// </summary>
        /// <returns>Коллекция профилей импорта.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution()
        {
            return Schema == null
                ? new List<ExcelImportProfile>()
                : _importPipeline.GetProfilesForExecution(Schema);
        }

        /// <summary>
        /// Проверяет текущую схему импорта.
        /// </summary>
        /// <returns>Результат проверки.</returns>
        public ExcelImportValidationResult Validate()
        {
            return Schema == null
                ? new ExcelImportValidationResult()
                : _importPipeline.Validate(Schema);
        }

        /// <summary>
        /// Проверяет настройку профиля импорта.
        /// </summary>
        /// <param name="profile">Профиль импорта.</param>
        /// <returns>Результат проверки.</returns>
        public ExcelImportValidationResult ValidateProfileConfiguration(ExcelImportProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            return _previewService.ValidateProfileConfiguration(profile);
        }

        /// <summary>
        /// Формирует JSON результата импорта.
        /// </summary>
        /// <returns>JSON результата импорта.</returns>
        public string BuildJson()
        {
            if (Schema == null)
            {
                throw new InvalidOperationException("Схема импорта не загружена.");
            }

            return _importPipeline.BuildJson(Schema);
        }

        /// <summary>
        /// Импортирует текущую схему в репозиторий.
        /// </summary>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="existingRoot">Существующий корень, в который выполняется импорт.</param>
        public void ImportToRepository(
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            TreeRootModel? existingRoot)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);

            if (Schema == null)
            {
                throw new InvalidOperationException("Схема импорта не загружена.");
            }

            _importPipeline.ImportToRepository(Schema, repository, repositoryService, existingRoot);
        }
    }
}
