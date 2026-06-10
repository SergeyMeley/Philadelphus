using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.Wpf.UI.Services;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления конструктора импорта из Excel.
    /// Заменяет логику code-behind окна ExcelImportDesignerWindow (см. docs/avalonia-migration/10).
    /// </summary>
    public class ExcelImportDesignerVM : ViewModelBase
    {
        private const string NoParentRelationOption = "(Нет родителя)";

        private readonly IServiceProvider _serviceProvider;
        private readonly ConversionService _conversionService;
        private readonly ExcelPreviewService _previewService;
        private readonly IExcelImportSchemaBuilder _schemaBuilder;
        private readonly IExcelImportSchemaTemplateStorage _templateStorage;
        private readonly ExcelImportPipeline _importPipeline;
        private readonly ExcelImportPresentationPipeline _presentationPipeline;
        private readonly IFileDialogService _fileDialogService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IWindowService _windowService;
        private readonly IRelayCommandFactory _commandFactory;

        // Рантайм-контекст, передаётся через Initialize после создания VM.
        private ShrubModel? _shrub;
        private PhiladelphusRepositoryModel? _repository;
        private IPhiladelphusRepositoryService? _repositoryService;
        private Action? _refreshRepositoryView;

        // Состояние редактирования схемы.
        private string _selectedFilePath = string.Empty;
        private ExcelPreviewWorkbookInfo? _workbookPreview;
        private ExcelImportSchema? _schema;
        private ExcelImportSheetSchema? _currentSheet;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportDesignerVM" />.
        /// </summary>
        public ExcelImportDesignerVM(
            IServiceProvider serviceProvider,
            ConversionService conversionService,
            ExcelPreviewService previewService,
            IExcelImportSchemaBuilder schemaBuilder,
            IExcelImportSchemaTemplateStorage templateStorage,
            ExcelImportPipeline importPipeline,
            ExcelImportPresentationPipeline presentationPipeline,
            IFileDialogService fileDialogService,
            IMessageDialogService messageDialogService,
            IDispatcherService dispatcherService,
            IWindowService windowService,
            IRelayCommandFactory commandFactory)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(conversionService);
            ArgumentNullException.ThrowIfNull(previewService);
            ArgumentNullException.ThrowIfNull(schemaBuilder);
            ArgumentNullException.ThrowIfNull(templateStorage);
            ArgumentNullException.ThrowIfNull(importPipeline);
            ArgumentNullException.ThrowIfNull(presentationPipeline);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(messageDialogService);
            ArgumentNullException.ThrowIfNull(dispatcherService);
            ArgumentNullException.ThrowIfNull(windowService);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _serviceProvider = serviceProvider;
            _conversionService = conversionService;
            _previewService = previewService;
            _schemaBuilder = schemaBuilder;
            _templateStorage = templateStorage;
            _importPipeline = importPipeline;
            _presentationPipeline = presentationPipeline;
            _fileDialogService = fileDialogService;
            _messageDialogService = messageDialogService;
            _dispatcherService = dispatcherService;
            _windowService = windowService;
            _commandFactory = commandFactory;
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
        /// Задаёт рантайм-контекст конструктора импорта.
        /// </summary>
        /// <param name="shrub">Рабочее дерево активного репозитория.</param>
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

            _shrub = shrub;
            _repository = repository;
            _repositoryService = repositoryService;
            _refreshRepositoryView = refreshRepositoryView;

            // TODO (Фаза1/п2): инициализация списка корней, черновой схемы и привязок.
        }
    }
}
