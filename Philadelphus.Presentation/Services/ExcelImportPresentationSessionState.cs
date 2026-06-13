using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Services
{
    /// <summary>
    /// Обертка Excel-сессии, добавляющая WPF-предпросмотр репозитория.
    /// </summary>
    public class ExcelImportPresentationSessionState
    {
        private readonly ExcelImportSessionState _session;
        private readonly ExcelImportPresentationPipeline _presentationPipeline;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportPresentationSessionState" />.
        /// </summary>
        /// <param name="session">Состояние Excel-импорта без привязки к UI.</param>
        /// <param name="presentationPipeline">WPF-пайплайн предпросмотра Excel-импорта.</param>
        public ExcelImportPresentationSessionState(
            ExcelImportSessionState session,
            ExcelImportPresentationPipeline presentationPipeline)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(presentationPipeline);

            _session = session;
            _presentationPipeline = presentationPipeline;
        }

        /// <summary>
        /// Путь к выбранному Excel-файлу.
        /// </summary>
        public string SelectedFilePath => _session.SelectedFilePath;

        /// <summary>
        /// Предпросмотр структуры Excel-книги.
        /// </summary>
        public ExcelPreviewWorkbookInfo? WorkbookPreview => _session.WorkbookPreview;

        /// <summary>
        /// Текущая схема импорта Excel.
        /// </summary>
        public ExcelImportSchema? Schema => _session.Schema;

        /// <summary>
        /// Текущий лист схемы импорта.
        /// </summary>
        public ExcelImportSheetSchema? CurrentSheet
        {
            get => _session.CurrentSheet;
            set => _session.CurrentSheet = value;
        }

        /// <summary>
        /// Загружает Excel-книгу и создает черновую схему импорта.
        /// </summary>
        /// <param name="filePath">Путь к Excel-файлу.</param>
        /// <param name="rootName">Наименование корня по умолчанию.</param>
        public void LoadWorkbook(string filePath, string rootName)
        {
            _session.LoadWorkbook(filePath, rootName);
        }

        /// <summary>
        /// Использует готовую схему импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        public void UseSchema(ExcelImportSchema schema)
        {
            _session.UseSchema(schema);
        }

        /// <summary>
        /// Возвращает предпросмотр данных листа.
        /// </summary>
        /// <param name="sheet">Лист схемы импорта.</param>
        /// <returns>Таблица предпросмотра.</returns>
        public ExcelPreviewTable GetPreview(ExcelImportSheetSchema sheet)
        {
            return _session.GetPreview(sheet);
        }

        /// <summary>
        /// Возвращает лист схемы по имени источника.
        /// </summary>
        /// <param name="sourceName">Имя источника Excel.</param>
        /// <returns>Лист схемы или null.</returns>
        public ExcelImportSheetSchema? GetSheet(string? sourceName)
        {
            return _session.GetSheet(sourceName);
        }

        /// <summary>
        /// Включает или отключает все листы схемы.
        /// </summary>
        /// <param name="isEnabled">Признак включения листов.</param>
        public void SetAllSheetsEnabled(bool isEnabled)
        {
            _session.SetAllSheetsEnabled(isEnabled);
        }

        /// <summary>
        /// Синхронизирует настройки корня со схемой импорта.
        /// </summary>
        /// <param name="createNewRoot">Признак создания нового корня.</param>
        /// <param name="rootName">Наименование корня.</param>
        public void SyncRootSettings(bool createNewRoot, string rootName)
        {
            _session.SyncRootSettings(createNewRoot, rootName);
        }

        /// <summary>
        /// Возвращает профили, включенные в импорт.
        /// </summary>
        /// <returns>Коллекция профилей импорта.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution()
        {
            return _session.GetProfilesForExecution();
        }

        /// <summary>
        /// Проверяет текущую схему импорта.
        /// </summary>
        /// <returns>Результат проверки.</returns>
        public ExcelImportValidationResult Validate()
        {
            return _session.Validate();
        }

        /// <summary>
        /// Проверяет настройку профиля импорта.
        /// </summary>
        /// <param name="profile">Профиль импорта.</param>
        /// <returns>Результат проверки.</returns>
        public ExcelImportValidationResult ValidateProfileConfiguration(ExcelImportProfile profile)
        {
            return _session.ValidateProfileConfiguration(profile);
        }

        /// <summary>
        /// Формирует JSON результата импорта.
        /// </summary>
        /// <returns>JSON результата импорта.</returns>
        public string BuildJson()
        {
            return _session.BuildJson();
        }

        /// <summary>
        /// Строит модель предпросмотра репозитория.
        /// </summary>
        /// <param name="targetExistingRootName">Имя существующего корня, в который планируется импорт.</param>
        /// <returns>Модель предпросмотра обозревателя репозитория.</returns>
        public RepositoryExplorerControlVM BuildRepositoryPreview(string? targetExistingRootName)
        {
            if (Schema == null)
            {
                throw new InvalidOperationException("Схема импорта не загружена.");
            }

            return _presentationPipeline.BuildRepositoryPreview(Schema, targetExistingRootName);
        }

        /// <summary>
        /// Формирует краткое описание результата предпросмотра.
        /// </summary>
        /// <param name="previewViewModel">Модель предпросмотра обозревателя репозитория.</param>
        /// <returns>Краткое описание результата предпросмотра.</returns>
        public string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            return _presentationPipeline.BuildPreviewSummary(previewViewModel);
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
            _session.ImportToRepository(repository, repositoryService, existingRoot);
        }
    }
}
