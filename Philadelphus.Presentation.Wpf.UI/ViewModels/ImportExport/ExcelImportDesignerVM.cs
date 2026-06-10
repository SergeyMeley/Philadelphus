using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.Wpf.UI.Services;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
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
    /// Фаза 1 завершена (файл/схема/листы, поля листа, редактор связей, действия:
    /// предпросмотр/импорт/шаблоны/закрытие). Диаграмма (рисование + drag/zoom) остаётся во View
    /// и выносится в Фазе 3.
    /// </remarks>
    public class ExcelImportDesignerVM : ViewModelBase
    {
        private const string NoParentRelationOption = "(Нет родителя)";

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

        private ExcelImportRelationSchema? _selectedRelation;
        private IReadOnlyList<string> _relationChildSheets = Array.Empty<string>();
        private string? _selectedRelationChildSheet;
        private IReadOnlyList<string> _relationParentSheets = Array.Empty<string>();
        private string? _selectedRelationParentSheet;
        private IReadOnlyList<string> _relationParentKeys = Array.Empty<string>();
        private string? _selectedRelationParentKey;
        private IReadOnlyList<string> _relationChildKeys = Array.Empty<string>();
        private string? _selectedRelationChildKey;
        private bool _isUpdatingRelationControls;

        private RepositoryExplorerControlVM? _repositoryPreviewVM;
        private string _previewSummary = "Предпросмотр результата еще не построен.";

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
            AddRelationCommand = _commandFactory.Create(_ => AddRelation());
            RemoveRelationCommand = _commandFactory.Create(_ => RemoveRelation());
            RefreshPreviewCommand = _commandFactory.Create(_ => RefreshPreview());
            ImportCommand = _commandFactory.Create(_ => Import());
            LoadTemplateCommand = _commandFactory.Create(_ => LoadTemplate());
            SaveTemplateCommand = _commandFactory.Create(_ => SaveTemplate());
            CloseCommand = _commandFactory.Create(_ => Close());
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
        /// Команда добавления (настройки) связи.
        /// </summary>
        public ICommand AddRelationCommand { get; }

        /// <summary>
        /// Команда удаления связи выбранного дочернего листа.
        /// </summary>
        public ICommand RemoveRelationCommand { get; }

        /// <summary>
        /// Команда построения предпросмотра результата импорта.
        /// </summary>
        public ICommand RefreshPreviewCommand { get; }

        /// <summary>
        /// Команда выполнения импорта в репозиторий.
        /// </summary>
        public ICommand ImportCommand { get; }

        /// <summary>
        /// Команда загрузки шаблона схемы импорта.
        /// </summary>
        public ICommand LoadTemplateCommand { get; }

        /// <summary>
        /// Команда сохранения шаблона схемы импорта.
        /// </summary>
        public ICommand SaveTemplateCommand { get; }

        /// <summary>
        /// Команда закрытия окна конструктора.
        /// </summary>
        public ICommand CloseCommand { get; }

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
        /// Связи текущей схемы импорта.
        /// </summary>
        public IReadOnlyList<ExcelImportRelationSchema> Relations
            => _session.Schema?.Relations ?? (IReadOnlyList<ExcelImportRelationSchema>)Array.Empty<ExcelImportRelationSchema>();

        /// <summary>
        /// Выбранная связь.
        /// </summary>
        public ExcelImportRelationSchema? SelectedRelation
        {
            get => _selectedRelation;
            set
            {
                if (SetProperty(ref _selectedRelation, value) && _isUpdatingRelationControls == false)
                {
                    BindRelationEditor(value?.ChildSourceName);
                }
            }
        }

        /// <summary>
        /// Доступные дочерние листы для настройки связи.
        /// </summary>
        public IReadOnlyList<string> RelationChildSheets
        {
            get => _relationChildSheets;
            private set => SetProperty(ref _relationChildSheets, value);
        }

        /// <summary>
        /// Выбранный дочерний лист связи.
        /// </summary>
        public string? SelectedRelationChildSheet
        {
            get => _selectedRelationChildSheet;
            set
            {
                if (SetProperty(ref _selectedRelationChildSheet, value) && _isUpdatingRelationControls == false)
                {
                    BindRelationEditor(value);
                }
            }
        }

        /// <summary>
        /// Доступные родительские листы для выбранного дочернего листа.
        /// </summary>
        public IReadOnlyList<string> RelationParentSheets
        {
            get => _relationParentSheets;
            private set => SetProperty(ref _relationParentSheets, value);
        }

        /// <summary>
        /// Выбранный родительский лист связи.
        /// </summary>
        public string? SelectedRelationParentSheet
        {
            get => _selectedRelationParentSheet;
            set
            {
                if (SetProperty(ref _selectedRelationParentSheet, value) && _isUpdatingRelationControls == false)
                {
                    var childSheet = _session.GetSheet(_selectedRelationChildSheet);
                    if (childSheet == null)
                        return;

                    if (TryApplyRelationParent(childSheet, value, selectChildSheet: false) == false)
                    {
                        BindRelationEditor(childSheet.SourceName);
                    }
                }
            }
        }

        /// <summary>
        /// Доступные ключевые колонки родителя.
        /// </summary>
        public IReadOnlyList<string> RelationParentKeys
        {
            get => _relationParentKeys;
            private set => SetProperty(ref _relationParentKeys, value);
        }

        /// <summary>
        /// Выбранная ключевая колонка родителя.
        /// </summary>
        public string? SelectedRelationParentKey
        {
            get => _selectedRelationParentKey;
            set
            {
                if (SetProperty(ref _selectedRelationParentKey, value) && _isUpdatingRelationControls == false)
                {
                    var childSheet = _session.GetSheet(_selectedRelationChildSheet);
                    if (childSheet == null)
                        return;

                    ExcelImportProfileEditorHelper.SetRelationParentKey(childSheet, value);
                    RefreshRelationUi(childSheet, bindEditor: false, selectChildSheet: false);
                }
            }
        }

        /// <summary>
        /// Доступные ключевые колонки дочернего листа.
        /// </summary>
        public IReadOnlyList<string> RelationChildKeys
        {
            get => _relationChildKeys;
            private set => SetProperty(ref _relationChildKeys, value);
        }

        /// <summary>
        /// Выбранная ключевая колонка дочернего листа.
        /// </summary>
        public string? SelectedRelationChildKey
        {
            get => _selectedRelationChildKey;
            set
            {
                if (SetProperty(ref _selectedRelationChildKey, value) && _isUpdatingRelationControls == false)
                {
                    var childSheet = _session.GetSheet(_selectedRelationChildSheet);
                    if (childSheet == null)
                        return;

                    ExcelImportProfileEditorHelper.SetRelationChildKey(childSheet, value);
                    RefreshRelationUi(childSheet, bindEditor: false, selectChildSheet: false);
                }
            }
        }

        /// <summary>
        /// Модель предпросмотра результирующего репозитория.
        /// </summary>
        public RepositoryExplorerControlVM? RepositoryPreviewVM
        {
            get => _repositoryPreviewVM;
            private set => SetProperty(ref _repositoryPreviewVM, value);
        }

        /// <summary>
        /// Краткая сводка предпросмотра результата.
        /// </summary>
        public string PreviewSummary
        {
            get => _previewSummary;
            private set => SetProperty(ref _previewSummary, value);
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

        /// <summary>
        /// Обрабатывает изменение признака включения листа в импорт.
        /// </summary>
        public void OnSheetEnabledChanged()
        {
            RefreshRelationViews();
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
                ApplyLoadedSchema();
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowError($"Не удалось загрузить книгу: {ex.Message}", "Ошибка");
            }
        }

        private void ApplyLoadedSchema()
        {
            var schema = _session.Schema;
            if (schema == null)
                return;

            schema.CreateNewRoot = true;
            RootName = schema.RootName;
            FilePathDisplay = string.IsNullOrWhiteSpace(_session.SelectedFilePath)
                ? "Файл не выбран"
                : Path.GetFileName(_session.SelectedFilePath);

            OnPropertyChanged(nameof(Sheets));
            RefreshRelationViews();
            SelectedSheet = Sheets.FirstOrDefault();
            ClearPreviewResult();
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

        private void RefreshRelationViews()
        {
            var schema = _session.Schema;
            if (schema == null)
                return;

            ExcelImportSchemaNormalizer.RefreshRelationProjection(schema);
            OnPropertyChanged(nameof(Relations));
        }

        private void BindRelationEditor(string? childSourceName)
        {
            _isUpdatingRelationControls = true;
            try
            {
                var schema = _session.Schema;
                if (schema == null)
                    return;

                RelationChildSheets = schema.Sheets.Select(x => x.SourceName).ToList();
                SelectedRelationChildSheet = string.IsNullOrWhiteSpace(childSourceName) ? null : childSourceName;

                var childSheet = _session.GetSheet(childSourceName);
                if (childSheet == null)
                {
                    RelationParentSheets = Array.Empty<string>();
                    RelationParentKeys = Array.Empty<string>();
                    RelationChildKeys = Array.Empty<string>();
                    SelectedRelationParentSheet = null;
                    SelectedRelationParentKey = null;
                    SelectedRelationChildKey = null;
                    return;
                }

                var parentOptions = ExcelImportProfileEditorHelper.BuildParentSourceOptions(
                    schema.Sheets.Select(x => x.SourceName),
                    childSheet.SourceName,
                    NoParentRelationOption);
                RelationParentSheets = parentOptions;
                SelectedRelationParentSheet = ExcelImportProfileEditorHelper.GetSelectedParentOption(
                    childSheet.Profile.Relation.ParentSourceName,
                    parentOptions,
                    NoParentRelationOption);

                RelationChildKeys = ExcelImportProfileEditorHelper.BuildHeaderOptions(childSheet.Profile.Columns, sort: false);
                SelectedRelationChildKey = string.IsNullOrWhiteSpace(childSheet.Profile.Relation.ChildKeyColumnName)
                    ? null
                    : childSheet.Profile.Relation.ChildKeyColumnName;

                RefreshRelationParentKeyOptions(childSheet);
            }
            finally
            {
                _isUpdatingRelationControls = false;
            }
        }

        private void RefreshRelationParentKeyOptions(ExcelImportSheetSchema childSheet)
        {
            var parentSheet = _session.GetSheet(childSheet.Profile.Relation.ParentSourceName);
            if (parentSheet == null)
            {
                RelationParentKeys = Array.Empty<string>();
                SelectedRelationParentKey = null;
                return;
            }

            RelationParentKeys = ExcelImportProfileEditorHelper.BuildHeaderOptions(parentSheet.Profile.Columns, sort: false);
            SelectedRelationParentKey = string.IsNullOrWhiteSpace(childSheet.Profile.Relation.ParentKeyColumnName)
                ? null
                : childSheet.Profile.Relation.ParentKeyColumnName;
        }

        private void RefreshRelationUi(ExcelImportSheetSchema? childSheet, bool bindEditor, bool selectChildSheet)
        {
            RefreshRelationViews();

            var selectionChanged = false;
            if (childSheet != null && selectChildSheet && ReferenceEquals(SelectedSheet, childSheet) == false)
            {
                SelectedSheet = childSheet;
                selectionChanged = true;
            }

            if (childSheet != null && bindEditor && selectionChanged == false)
            {
                BindRelationEditor(childSheet.SourceName);
            }

            ClearPreviewResult();
        }

        private bool TryApplyRelationParent(ExcelImportSheetSchema childSheet, string? parentSourceName, bool selectChildSheet)
        {
            var schema = _session.Schema;
            if (schema == null)
                return false;

            if (ExcelImportProfileEditorHelper.TrySetRelationParent(
                    schema.Sheets,
                    childSheet,
                    parentSourceName,
                    NoParentRelationOption,
                    out var errorMessage) == false)
            {
                _messageDialogService.ShowWarning(errorMessage, "Связи");
                return false;
            }

            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet);
            return true;
        }

        /// <summary>
        /// Удаляет связь дочернего листа с родителем (используется диаграммой).
        /// </summary>
        public void ClearSheetRelation(ExcelImportSheetSchema childSheet)
        {
            ArgumentNullException.ThrowIfNull(childSheet);

            ExcelImportProfileEditorHelper.ClearRelation(childSheet);
            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet: false);
        }

        /// <summary>
        /// Применяет связь по перетянутым колонкам (используется диаграммой при drag-drop).
        /// </summary>
        public bool ApplyRelationFromColumns(
            ExcelImportSheetSchema childSheet,
            string parentSourceName,
            string parentKeyColumnName,
            string childKeyColumnName,
            bool selectChildSheet)
        {
            ArgumentNullException.ThrowIfNull(childSheet);

            var schema = _session.Schema;
            if (schema == null)
                return false;

            if (ExcelImportProfileEditorHelper.TrySetRelationColumns(
                    schema.Sheets,
                    childSheet,
                    parentSourceName,
                    parentKeyColumnName,
                    childKeyColumnName,
                    out var errorMessage) == false)
            {
                _messageDialogService.ShowWarning(errorMessage, "Связи");
                return false;
            }

            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet);
            return true;
        }

        private void AddRelation()
        {
            var schema = _session.Schema;
            if (schema == null)
                return;

            var targetSheet = schema.Sheets.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Profile.Relation.ParentSourceName));
            if (targetSheet == null)
            {
                _messageDialogService.ShowInformation("Все листы уже имеют настроенную связь или книга не загружена.", "Связи");
                return;
            }

            BindRelationEditor(targetSheet.SourceName);
        }

        private void RemoveRelation()
        {
            var childSheet = _session.GetSheet(_selectedRelationChildSheet);
            if (childSheet == null)
                return;

            ClearSheetRelation(childSheet);
        }

        private void RefreshPreview()
        {
            try
            {
                if (TryGetImportRootName(out var rootName) == false)
                    return;

                if (TrySyncSchemaForExecution(rootName) == false)
                    return;

                var profiles = _session.GetProfilesForExecution();
                var validationResult = _session.Validate();
                if (validationResult.HasErrors)
                {
                    _messageDialogService.ShowWarning(ExcelImportValidationMessageBuilder.Build(validationResult), "Ошибка проверки данных");
                    return;
                }

                if (profiles.Count == 0)
                {
                    _messageDialogService.ShowWarning("Не выбраны источники Excel для предпросмотра импорта.", "Ошибка");
                    return;
                }

                var previewVm = _session.BuildRepositoryPreview(null);
                RepositoryPreviewVM = previewVm;
                PreviewSummary = _session.BuildPreviewSummary(previewVm);
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowError($"Не удалось построить предпросмотр: {ex.Message}", "Ошибка");
            }
        }

        private void Import()
        {
            if (_repository == null || _repositoryService == null)
            {
                _messageDialogService.ShowError("Не инициализирован контекст активного репозитория.", "Ошибка");
                return;
            }

            try
            {
                if (TryGetImportRootName(out var rootName) == false)
                    return;

                if (TrySyncSchemaForExecution(rootName) == false)
                    return;

                var profiles = _session.GetProfilesForExecution();
                if (profiles.Count == 0)
                {
                    _messageDialogService.ShowWarning("Не выбраны источники Excel для импорта.", "Ошибка");
                    return;
                }

                var validationResult = _session.Validate();
                if (validationResult.HasErrors)
                {
                    _messageDialogService.ShowWarning(ExcelImportValidationMessageBuilder.Build(validationResult), "Ошибка проверки данных");
                    return;
                }

                var progressWindow = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                progressWindow.Initialize("Импорт дерева", "Подготовка операции...");
                progressWindow.Show();

                var repository = _repository;
                var repositoryService = _repositoryService;
                var refreshRepositoryView = _refreshRepositoryView;

                CompletedImport = true;
                _windowService.Close(this);

                IProgress<string> statusProgress = new Progress<string>(status => progressWindow.UpdateStatus(status));
                _ = Task.Run(() =>
                {
                    try
                    {
                        statusProgress.Report("Чтение Excel и формирование PHJSON...");
                        statusProgress.Report("Импорт дерева в Чубушник...");
                        _session.ImportToRepository(repository, repositoryService, null);
                        _dispatcherService.Invoke(() =>
                        {
                            refreshRepositoryView?.Invoke();
                            progressWindow.Complete("Импорт завершен. Сохраните репозиторий для записи в хранилище.");
                        });
                    }
                    catch (Exception ex)
                    {
                        _dispatcherService.Invoke(() => progressWindow.Fail($"Ошибка импорта: {ex.Message}"));
                    }
                });
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowError($"Не удалось выполнить импорт: {ex.Message}", "Ошибка");
            }
        }

        private void LoadTemplate()
        {
            var filePath = _fileDialogService.OpenImportSchemaFile();
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var loadedSchema = _templateStorage.Load(filePath);
            var schemaFilePath = string.IsNullOrWhiteSpace(_session.SelectedFilePath)
                ? loadedSchema.SourceFilePath
                : _session.SelectedFilePath;

            if (string.IsNullOrWhiteSpace(schemaFilePath) || File.Exists(schemaFilePath) == false)
            {
                _messageDialogService.ShowWarning(
                    "Для загруженного шаблона не найден Excel-файл. Сначала выберите книгу вручную.",
                    "Шаблон импорта");
                _session.UseSchema(loadedSchema);
                ApplyLoadedSchema();
                return;
            }

            loadedSchema.SourceFilePath = schemaFilePath;
            loadedSchema.RootName = string.IsNullOrWhiteSpace(loadedSchema.RootName)
                ? Path.GetFileNameWithoutExtension(schemaFilePath)
                : loadedSchema.RootName;
            _session.UseSchema(loadedSchema);
            ApplyLoadedSchema();
        }

        private void SaveTemplate()
        {
            var schema = _session.Schema;
            if (schema == null)
            {
                _messageDialogService.ShowInformation("Сначала выберите Excel-файл и настройте схему импорта.", "Шаблон импорта");
                return;
            }

            ExcelImportSchemaNormalizer.RefreshRelationProjection(schema);
            _session.SyncRootSettings(createNewRoot: true, RootName?.Trim() ?? string.Empty);

            var defaultFileName = $"{schema.Name}.phimportschema.json";
            var savePath = _fileDialogService.SaveImportSchemaFile(defaultFileName);
            if (string.IsNullOrWhiteSpace(savePath))
                return;

            _templateStorage.Save(savePath, schema);
            _messageDialogService.ShowInformation("Шаблон схемы импорта сохранен.", "Шаблон импорта");
        }

        private void Close()
        {
            _windowService.Close(this);
        }

        private bool TryGetImportRootName(out string rootName)
        {
            rootName = string.Empty;

            if (string.IsNullOrWhiteSpace(_session.SelectedFilePath))
            {
                _messageDialogService.ShowWarning("Сначала выберите файл Excel.", "Ошибка");
                return false;
            }

            rootName = RootName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rootName))
            {
                _messageDialogService.ShowWarning("Укажите наименование корня.", "Ошибка");
                return false;
            }

            return true;
        }

        private bool TrySyncSchemaForExecution(string rootName)
        {
            var schema = _session.Schema;
            if (schema == null)
            {
                _messageDialogService.ShowWarning("Сначала выберите Excel-файл и настройте схему импорта.", "Ошибка");
                return false;
            }

            _session.SyncRootSettings(createNewRoot: true, rootName);
            ExcelImportSchemaNormalizer.NormalizeForExecution(schema);
            RefreshRelationViews();
            return true;
        }

        private void ClearPreviewResult()
        {
            RepositoryPreviewVM = null;
            PreviewSummary = "Предпросмотр результата еще не построен.";
        }
    }
}
