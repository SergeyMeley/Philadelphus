using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.Wpf.UI.Services;
using System.Data;
using System.IO;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления конструктора импорта из Excel.
    /// Заменяет логику code-behind окна ExcelImportDesignerWindow (см. docs/avalonia-migration/10).
    /// </summary>
    /// <remarks>
    /// Фаза 1, группы 1-2 (файл/схема/листы + поля выбранного листа). Редактор связей (группа 3),
    /// действия предпросмотр/импорт/шаблоны/закрытие (группа 4) и диаграмма — следующими порциями.
    /// </remarks>
    public class ExcelImportDesignerVM : ViewModelBase
    {
        private readonly ExcelImportPresentationSessionState _session;
        private readonly IExcelImportSchemaTemplateStorage _templateStorage;
        private readonly IFileDialogService _fileDialogService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IWindowService _windowService;
        private readonly IRelayCommandFactory _commandFactory;
        private readonly IServiceProvider _serviceProvider;

        // Рантайм-контекст, передаётся через Initialize после создания VM.
        private PhiladelphusRepositoryModel? _repository;
        private IPhiladelphusRepositoryService? _repositoryService;
        private Action? _refreshRepositoryView;

        private string _filePathDisplay = "Файл не выбран";
        private string _rootName = string.Empty;
        private ExcelImportSheetSchema? _selectedSheet;

        private string _currentSheetDisplayName = string.Empty;
        private string _currentSheetSource = string.Empty;
        private IReadOnlyList<string> _sheetKeyColumns = Array.Empty<string>();
        private string? _selectedSheetKeyColumn;
        private IReadOnlyList<ExcelImportColumnProfile> _sheetColumns = Array.Empty<ExcelImportColumnProfile>();
        private string _sheetPreviewInfo = "Лист не выбран.";
        private DataView? _sheetPreview;
        private bool _isUpdatingSheetControls;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportDesignerVM" />.
        /// </summary>
        public ExcelImportDesignerVM(
            ExcelImportPresentationSessionState session,
            IExcelImportSchemaTemplateStorage templateStorage,
            IFileDialogService fileDialogService,
            IMessageDialogService messageDialogService,
            IDispatcherService dispatcherService,
            IWindowService windowService,
            IRelayCommandFactory commandFactory,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(templateStorage);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(messageDialogService);
            ArgumentNullException.ThrowIfNull(dispatcherService);
            ArgumentNullException.ThrowIfNull(windowService);
            ArgumentNullException.ThrowIfNull(commandFactory);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _session = session;
            _templateStorage = templateStorage;
            _fileDialogService = fileDialogService;
            _messageDialogService = messageDialogService;
            _dispatcherService = dispatcherService;
            _windowService = windowService;
            _commandFactory = commandFactory;
            _serviceProvider = serviceProvider;

            SelectFileCommand = _commandFactory.Create(_ => SelectFile());
        }

        /// <summary>
        /// Признак того, что импорт был запущен и окно следует закрыть.
        /// </summary>
        public bool CompletedImport { get; private set; }

        /// <summary>
        /// Доступные роли колонок.
        /// </summary>
        public Array AvailableColumnRoles => Enum.GetValues(typeof(ExcelImportColumnRole));

        /// <summary>
        /// Доступные области определения.
        /// </summary>
        public List<ExcelImportDefinitionScopeItem> AvailableDefinitionScopes { get; }
            = ExcelImportDisplayOptionsHelper.CreateDefinitionScopeItems();

        /// <summary>
        /// Доступные режимы значения.
        /// </summary>
        public List<ExcelImportValueModeItem> AvailableValueModes { get; }
            = ExcelImportDisplayOptionsHelper.CreateValueModeItems();

        /// <summary>
        /// Доступные виды сущностей.
        /// </summary>
        public List<ExcelImportEntityKindItem> AvailableEntityKinds { get; }
            = ExcelImportDisplayOptionsHelper.CreateEntityKindItems();

        /// <summary>
        /// Команда выбора Excel-файла.
        /// </summary>
        public ICommand SelectFileCommand { get; }

        /// <summary>
        /// Отображаемое имя выбранного Excel-файла.
        /// </summary>
        public string FilePathDisplay
        {
            get => _filePathDisplay;
            private set => SetProperty(ref _filePathDisplay, value);
        }

        /// <summary>
        /// Наименование создаваемого корня.
        /// </summary>
        public string RootName
        {
            get => _rootName;
            set => SetProperty(ref _rootName, value);
        }

        /// <summary>
        /// Листы текущей схемы импорта.
        /// </summary>
        public IReadOnlyList<ExcelImportSheetSchema> Sheets
            => _session.Schema?.Sheets ?? (IReadOnlyList<ExcelImportSheetSchema>)Array.Empty<ExcelImportSheetSchema>();

        /// <summary>
        /// Выбранный лист схемы.
        /// </summary>
        public ExcelImportSheetSchema? SelectedSheet
        {
            get => _selectedSheet;
            set
            {
                if (SetProperty(ref _selectedSheet, value))
                {
                    _session.CurrentSheet = value;
                    BindCurrentSheet();
                }
            }
        }

        /// <summary>
        /// Отображаемое имя выбранного листа.
        /// </summary>
        public string CurrentSheetDisplayName
        {
            get => _currentSheetDisplayName;
            set
            {
                if (SetProperty(ref _currentSheetDisplayName, value)
                    && _isUpdatingSheetControls == false
                    && _session.CurrentSheet != null)
                {
                    _session.CurrentSheet.DisplayName = value?.Trim() ?? string.Empty;
                    OnPropertyChanged(nameof(Sheets));
                }
            }
        }

        /// <summary>
        /// Имя источника выбранного листа.
        /// </summary>
        public string CurrentSheetSource
        {
            get => _currentSheetSource;
            private set => SetProperty(ref _currentSheetSource, value);
        }

        /// <summary>
        /// Доступные колонки-ключи выбранного листа.
        /// </summary>
        public IReadOnlyList<string> SheetKeyColumns
        {
            get => _sheetKeyColumns;
            private set => SetProperty(ref _sheetKeyColumns, value);
        }

        /// <summary>
        /// Выбранная колонка-ключ строки.
        /// </summary>
        public string? SelectedSheetKeyColumn
        {
            get => _selectedSheetKeyColumn;
            set
            {
                if (SetProperty(ref _selectedSheetKeyColumn, value)
                    && _isUpdatingSheetControls == false
                    && _session.CurrentSheet != null)
                {
                    _session.CurrentSheet.RowKeyColumnName = value ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Колонки профиля выбранного листа.
        /// </summary>
        public IReadOnlyList<ExcelImportColumnProfile> SheetColumns
        {
            get => _sheetColumns;
            private set => SetProperty(ref _sheetColumns, value);
        }

        /// <summary>
        /// Вид сущности выбранного листа (по текущей логике всегда узел).
        /// </summary>
        public ExcelImportEntityKind SelectedSheetEntityKind
        {
            get => _session.CurrentSheet?.EntityKind ?? ExcelImportEntityKind.Node;
            set
            {
                if (_isUpdatingSheetControls || _session.CurrentSheet == null)
                    return;

                ApplySheetEntityKind(_session.CurrentSheet);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sheets));
            }
        }

        /// <summary>
        /// Текстовая сводка предпросмотра листа.
        /// </summary>
        public string SheetPreviewInfo
        {
            get => _sheetPreviewInfo;
            private set => SetProperty(ref _sheetPreviewInfo, value);
        }

        /// <summary>
        /// Табличный предпросмотр данных листа.
        /// </summary>
        public DataView? SheetPreview
        {
            get => _sheetPreview;
            private set => SetProperty(ref _sheetPreview, value);
        }

        /// <summary>
        /// Задаёт рантайм-контекст конструктора импорта.
        /// </summary>
        /// <param name="shrub">Рабочее дерево активного репозитория (резерв для будущих сценариев).</param>
        /// <param name="repository">Активный репозиторий.</param>
        /// <param name="repositoryService">Сервис работы с репозиторием.</param>
        /// <param name="refreshRepositoryView">Колбэк обновления представления репозитория.</param>
        public void Initialize(
            ShrubModel shrub,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView)
        {
            ArgumentNullException.ThrowIfNull(shrub);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(refreshRepositoryView);

            _repository = repository;
            _repositoryService = repositoryService;
            _refreshRepositoryView = refreshRepositoryView;
        }

        /// <summary>
        /// Обрабатывает завершение редактирования ячейки колонки листа
        /// (вызывается из представления при CellEditEnding).
        /// </summary>
        public void OnSheetColumnEdited()
        {
            if (_session.CurrentSheet == null)
                return;

            ExcelImportProfileEditorHelper.SyncSheetFieldsFromProfile(_session.CurrentSheet);
            BindCurrentSheet();
        }

        private void SelectFile()
        {
            var filePath = _fileDialogService.OpenExcelFile();
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                RootName = Path.GetFileNameWithoutExtension(filePath);
                _session.LoadWorkbook(filePath, RootName);
                FilePathDisplay = Path.GetFileName(filePath);
                OnPropertyChanged(nameof(Sheets));
                SelectedSheet = Sheets.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowError($"Не удалось загрузить книгу: {ex.Message}", "Ошибка");
            }
        }

        private void BindCurrentSheet()
        {
            _isUpdatingSheetControls = true;
            try
            {
                var sheet = _session.CurrentSheet;
                if (sheet == null || string.IsNullOrWhiteSpace(_session.SelectedFilePath))
                {
                    CurrentSheetDisplayName = string.Empty;
                    CurrentSheetSource = string.Empty;
                    SheetKeyColumns = Array.Empty<string>();
                    SelectedSheetKeyColumn = null;
                    SheetColumns = Array.Empty<ExcelImportColumnProfile>();
                    SheetPreview = null;
                    SheetPreviewInfo = "Лист не выбран.";
                    return;
                }

                ExcelImportProfileEditorHelper.SyncSheetFieldsFromProfile(sheet);
                ApplySheetEntityKind(sheet);

                CurrentSheetDisplayName = sheet.DisplayName;
                CurrentSheetSource = sheet.SourceName;
                SheetKeyColumns = ExcelImportProfileEditorHelper.BuildHeaderOptions(sheet.Profile.Columns, sort: false);
                SelectedSheetKeyColumn = string.IsNullOrWhiteSpace(sheet.RowKeyColumnName) ? null : sheet.RowKeyColumnName;
                SheetColumns = sheet.Profile.Columns;

                var preview = _session.GetPreview(sheet);
                SheetPreviewInfo = $"Лист: {preview.SourceName}. Строк данных: {preview.TotalRowCount}. Колонок: {preview.TotalColumnCount}.";
                SheetPreview = ExcelPreviewTableBuilder.Build(preview).DefaultView;
            }
            finally
            {
                _isUpdatingSheetControls = false;
            }

            BindRelationEditor(_session.CurrentSheet?.SourceName);
            OnPropertyChanged(nameof(SelectedSheetEntityKind));
            OnPropertyChanged(nameof(Sheets));
        }

        private static void ApplySheetEntityKind(ExcelImportSheetSchema sheet)
        {
            sheet.EntityKind = ExcelImportEntityKind.Node;
            sheet.Profile.EntityKind = ExcelImportEntityKind.Node;
        }

        private void BindRelationEditor(string? childSourceName)
        {
            // TODO (Фаза1/п2-группа3): редактор связей (child/parent sheet + ключи) через
            // ExcelImportProfileEditorHelper.BuildParentSourceOptions/BuildHeaderOptions и т.д.
        }
    }
}
