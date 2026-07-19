using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Models.LeavePolymorphism;
using Philadelphus.Presentation.Models.Tables;
using Philadelphus.Presentation.Models.Dialogs;
using Philadelphus.Core.Domain.Relations;
using Philadelphus.Presentation.Services;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Services.Tables;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Presentation.Factories.Interfaces;
using Serilog;
using IAsyncRelayCommand = Philadelphus.Presentation.Infrastructure.IAsyncRelayCommand;
using IRelayCommand = Philadelphus.Presentation.Infrastructure.IRelayCommand;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления обозревателя репозитория Чубушника.
    /// </summary>
    public  class RepositoryExplorerControlVM : ControlBaseVM, IDisposable
    {
        #region [ Props ]

        private bool _isDisposed;

        private readonly IPhiladelphusRepositoryService _service;
        private readonly IFileDialogService _fileDialogService;
        private readonly IRelayCommandFactory _commandFactory;
        private readonly IAsyncRelayCommandFactory _asyncCommandFactory;
        private readonly IWindowService _windowService;
        private readonly IDialogService _dialogService;
        private readonly IDataStorageSelectionDialogService _dataStorageSelectionDialogService;
        private readonly IRelationDeletionConfirmationService _relationDeletionConfirmationService;
        private readonly IRepositoryRelationsService _relationsService;
        private readonly ILeavePolymorphismService _leavePolymorphismService;
        private readonly ILeavePolymorphismChangeCoordinator _leavePolymorphismChangeCoordinator;
        private readonly SemaphoreSlim _repositoryLoadSemaphore = new SemaphoreSlim(1, 1);
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
       
        private PhiladelphusRepositoryVM _philadelphusRepositoryVM;     // TODO: Тех. долг. Вернуть readonly
        private int _repositoryLoadVersion;

        private IMainEntityVM<IMainEntityModel>? _selectedRepositoryMember;
        private IMainEntityVM<IMainEntityModel>? _selectedRepositoryTreeMember;
        private TreeLeaveVM? _currentLeave;
        private ILeaveParent? _currentLeavesOwner;
        private IReadOnlyList<ChildCollectionTableColumn> _childCollectionTableColumns = Array.Empty<ChildCollectionTableColumn>();
        private IReadOnlyList<ChildCollectionTableRow> _childCollectionTableRows = Array.Empty<ChildCollectionTableRow>();

        // Sequence редактируется прямо в таблице. Пересортировку откладываем до ухода фокуса,
        // чтобы строка не меняла позицию во время ввода значения.
        private bool _isChildCollectionTableOrderStale;

        private IAsyncRelayCommand? _getWorkCommand;
        private IRelayCommand? _saveCommand;
        private IAsyncRelayCommand? _createWorkingTreeCommand;
        private IRelayCommand? _createNodeCommand;
        private IRelayCommand? _createLeaveCommand;
        private IRelayCommand? _createAttributeCommand;
        private IAsyncRelayCommand? _applyPolymorphicParentCommand;
        private IRelayCommand? _createPolymorphicParentCommand;
        private IAsyncRelayCommand? _softDeleteRepositoryMemberCommand;
        private IAsyncRelayCommand? _hardDeleteRepositoryMemberCommand;
        private IAsyncRelayCommand? _softDeleteRepositoryMemberAttributeCommand;
        private IRelayCommand? _openModifyAttributesListWindowCommand;
        private IRelayCommand? _addAttributeValueCommand;
        private IRelayCommand? _removeAttributeValueCommand;
        private IRelayCommand? _protectCommand;
        private IRelayCommand? _rebuildChildCollectionTableIfOrderStaleCommand;

        public PhiladelphusRepositoryVM PhiladelphusRepositoryVM 
        { 
            get 
            { 
                return _philadelphusRepositoryVM; 
            }
        }

        public bool IsSystemBaseLeaveControlVisible
            => SelectedRepositoryMember?.Model is SystemBaseTreeLeaveModel;

        public bool IsParentControlVisible
            => SelectedRepositoryMember is TreeRootVM or TreeNodeVM;

        /// <summary>
        /// Имя открытого репозитория Чубушника.
        /// </summary>
        public string CurentRepositoryName { get => _philadelphusRepositoryVM.Name; }

        public IMainEntityVM<IMainEntityModel>? SelectedRepositoryMember
        {
            get
            {
                return _selectedRepositoryMember;
            }
            set
            {
                _selectedRepositoryMember = value;
                ConfigureLeavePolymorphismCommands(value);
                SynchronizeTreeSelection(value);
                UpdateCurrentLeavesOwner(value);
                RebuildChildCollectionTable();
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(SelectedRepositoryMember));
                OnPropertyChanged(nameof(IsSystemBaseLeaveControlVisible));
                OnPropertyChanged(nameof(IsParentControlVisible));
                _softDeleteRepositoryMemberCommand?.RaiseCanExecuteChanged();
                _hardDeleteRepositoryMemberCommand?.RaiseCanExecuteChanged();
                FormulaBarVM.SelectedFormulaAttribute = null;
                FormulaBarVM.NotifySelectedRepositoryMemberChanged();
                RelationsVM.Refresh();
                NavigationVM.NotifySelectedRepositoryMemberChanged(value);
            }
        }

        /// <summary>
        /// Элемент, выделенный непосредственно в дереве обозревателя репозитория.
        /// </summary>
        public IMainEntityVM<IMainEntityModel>? SelectedRepositoryTreeMember
        {
            get => _selectedRepositoryTreeMember;
            set
            {
                if (ReferenceEquals(_selectedRepositoryTreeMember, value))
                    return;

                _selectedRepositoryTreeMember = value;
                OnPropertyChanged(nameof(SelectedRepositoryTreeMember));
                SelectedRepositoryMember = value;
            }
        }

        /// <summary>
        /// Текущий лист.
        /// </summary>
        public TreeLeaveVM? CurrentLeave
        {
            get => _currentLeave;
            private set => SetProperty(ref _currentLeave, value);
        }

        /// <summary>
        /// Текущий владелец листьев, отображаемых в таблице листов.
        /// </summary>
        public ILeaveParent? CurrentLeavesOwner
        {
            get => _currentLeavesOwner;
            private set
            {
                if (SetProperty(ref _currentLeavesOwner, value))
                    NavigationVM.NotifyCurrentElementChanged();
            }
        }

        //public List<InfrastructureTypes> InfrastructureTypes
        //{
        //    get
        //    {
        //        return Enum.GetValues(typeof(InfrastructureTypes)).Cast<InfrastructureTypes>().ToList();
        //    }
        //}
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_selectedRepositoryMember == null)
                    return null;
                return PropertyGridHelper.GetProperties(_selectedRepositoryMember);
            }
        }

        /// <summary>
        /// Динамические колонки плоской таблицы наследников выбранного элемента.
        /// </summary>
        public IReadOnlyList<ChildCollectionTableColumn> ChildCollectionTableColumns
        {
            get => _childCollectionTableColumns;
        }

        /// <summary>
        /// Строки плоской таблицы наследников выбранного элемента.
        /// </summary>
        public IReadOnlyList<ChildCollectionTableRow> ChildCollectionTableRows
        {
            get => _childCollectionTableRows;
        }

        private int _currentElementTabIndex;

        /// <summary>
        /// Индекс выбранной вкладки сведений о текущем элементе.
        /// </summary>
        public int CurrentElementTabIndex { get => _currentElementTabIndex; set => SetProperty(ref _currentElementTabIndex, value); }

        /// <summary>
        /// Модель представления навигации по репозиторию.
        /// </summary>
        public RepositoryNavigationVM NavigationVM { get; }

        /// <summary>
        /// Модель представления дерева связей.
        /// </summary>
        public RepositoryRelationsControlVM RelationsVM { get; }

        private ExtensionsControlVM _extensionsControlVM;

        /// <summary>
        /// Модель представления панели расширений текущего обозревателя.
        /// </summary>
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }

        /// <summary>
        /// Модель представления строки формул обозревателя репозитория.
        /// </summary>
        public RepositoryFormulaBarVM FormulaBarVM { get; }

        private bool _isRepositoryLoading;
        public bool IsRepositoryLoading
        {
            get => _isRepositoryLoading;
            private set
            {
                if (_isRepositoryLoading == value)
                    return;

                _isRepositoryLoading = value;
                OnPropertyChanged(nameof(IsRepositoryLoading));
                OnPropertyChanged(nameof(IsRepositoryContentEnabled));
                OnPropertyChanged(nameof(IsRepositoryLoadingVisible));
                RaiseRepositoryCommandsCanExecuteChanged();
                FormulaBarVM.RaiseLoadingDependentCommandsCanExecuteChanged();
            }
        }

        /// <summary>
        /// Указывает, доступно ли содержимое репозитория для взаимодействия.
        /// </summary>
        public bool IsRepositoryContentEnabled => IsRepositoryLoading == false;

        public bool IsRepositoryLoadingVisible => IsRepositoryLoading;

        private bool _isReadOnly;

        /// <summary>
        /// Режим «только чтение» (например, предпросмотр импорта): редактирующие
        /// кнопки/команды недоступны, изменить содержимое нельзя.
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="extensionVMFactory">Фабрика создания модели представления расширений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="PhiladelphusRepositoryVM">Модель представления репозитория Чубушника.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <param name="asyncCommandFactory">Фабрика асинхронных команд.</param>
        /// <param name="navigationVMFactory">Фабрика модели навигации по репозиторию.</param>
        /// <param name="relationsVMFactory">Фабрика модели дерева связей.</param>
        /// <param name="relationsService">Сервис вычисления связей репозитория.</param>
        /// <param name="relationDeletionConfirmationService">Сервис подтверждения удаления связанных элементов.</param>
        /// <param name="leavePolymorphismService">Сервис runtime-связей полиморфных листов.</param>
        /// <param name="leavePolymorphismChangeCoordinator">Координатор интерактивных полиморфных изменений.</param>
        /// <param name="dialogService">Сервис диалогов подтверждения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RepositoryExplorerControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            IPhiladelphusRepositoryService service,
            FormulaAstEvaluator formulaEvaluator,
            FormulaRegistry formulaRegistry,
            IFormulaDiagnosticsReporter formulaDiagnosticsReporter,
            IExtensionsControlVMFactory extensionVMFactory,
            IApplicationCommandsVM applicationCommandsVM,
            PhiladelphusRepositoryVM PhiladelphusRepositoryVM,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IFileDialogService fileDialogService,
            IRelayCommandFactory commandFactory,
            IAsyncRelayCommandFactory asyncCommandFactory,
            IWindowService windowService,
            IDialogService dialogService,
            IDataStorageSelectionDialogService dataStorageSelectionDialogService,
            IRepositoryNavigationVMFactory navigationVMFactory,
            IRepositoryRelationsControlVMFactory relationsVMFactory,
            IRepositoryRelationsService relationsService,
            IRelationDeletionConfirmationService relationDeletionConfirmationService,
            ILeavePolymorphismService leavePolymorphismService,
            ILeavePolymorphismChangeCoordinator leavePolymorphismChangeCoordinator,
            bool loadOnStartup = true)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(formulaEvaluator);
            ArgumentNullException.ThrowIfNull(formulaRegistry);
            ArgumentNullException.ThrowIfNull(formulaDiagnosticsReporter);
            ArgumentNullException.ThrowIfNull(extensionVMFactory);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryVM);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(commandFactory);
            ArgumentNullException.ThrowIfNull(asyncCommandFactory);
            ArgumentNullException.ThrowIfNull(windowService);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(dataStorageSelectionDialogService);
            ArgumentNullException.ThrowIfNull(navigationVMFactory);
            ArgumentNullException.ThrowIfNull(relationsVMFactory);
            ArgumentNullException.ThrowIfNull(relationsService);
            ArgumentNullException.ThrowIfNull(relationDeletionConfirmationService);
            ArgumentNullException.ThrowIfNull(leavePolymorphismService);
            ArgumentNullException.ThrowIfNull(leavePolymorphismChangeCoordinator);

            _service = service;
            _fileDialogService = fileDialogService;
            _commandFactory = commandFactory;
            _asyncCommandFactory = asyncCommandFactory;
            _windowService = windowService;
            _dialogService = dialogService;
            _dataStorageSelectionDialogService = dataStorageSelectionDialogService;
            _relationsService = relationsService;
            _relationDeletionConfirmationService = relationDeletionConfirmationService;
            _leavePolymorphismService = leavePolymorphismService;
            _leavePolymorphismChangeCoordinator = leavePolymorphismChangeCoordinator;
            _extensionsControlVM = extensionVMFactory.Create(this);
            _philadelphusRepositoryVM = PhiladelphusRepositoryVM;
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            NavigationVM = navigationVMFactory.Create(this);
            RelationsVM = relationsVMFactory.Create(this, NavigationVM);
            FormulaBarVM = new RepositoryFormulaBarVM(
                this,
                service,
                formulaEvaluator,
                formulaRegistry,
                formulaDiagnosticsReporter,
                notificationService,
                applicationCommandsVM,
                commandFactory,
                asyncCommandFactory,
                leavePolymorphismChangeCoordinator);

            if (loadOnStartup)
            {
                _ = LoadPhiladelphusRepositoryOnStartupAsync();
            }

            _notificationService.SendTextMessage<RepositoryExplorerControlVM>("Обозреватель репозитория. Начало инициализации расширений.", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectories);
            _notificationService.SendTextMessage<RepositoryExplorerControlVM>($"Обозреватель репозитория. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт.).", NotificationCriticalLevelModel.Info);
        }

        #endregion

        #region [Commands]

        public IAsyncRelayCommand GetWorkCommand =>
            _getWorkCommand ??= _asyncCommandFactory.Create(
                ExecuteGetWorkAsync,
                _ => IsRepositoryLoading == false);

        public IRelayCommand SaveCommand =>
            _saveCommand ??= _commandFactory.Create(
                obj =>
                {
                    var repo = _philadelphusRepositoryVM.Model;
                    try
                    {
                        _service.SaveChanges(ref repo, SaveMode.WithContentAndMembers);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Ошибка сохранения изменений в БД.");
                        OnPropertyChanged(nameof(State));
                        NotifyChildsPropertyChangedRecursive();
                        return;
                    }

                    UpdateChildsCollection(_philadelphusRepositoryVM);   // TODO
                    OnPropertyChanged(nameof(State));
                    NotifyChildsPropertyChangedRecursive();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        public IAsyncRelayCommand CreateWorkingTreeCommand
        {
            get
            {
                return _createWorkingTreeCommand ??= _asyncCommandFactory.Create(
                    ExecuteCreateWorkingTreeAsync,
                    ce =>
                    {
                        return CanModifyRepository();
                    });
            }
        }

        public IRelayCommand CreateNodeCommand =>
            _createNodeCommand ??= _commandFactory.Create(
                obj =>
                {
                    if (_selectedRepositoryMember == null)
                        return;
                    if (_selectedRepositoryMember is INodeParent np)
                    {
                        np.CreateTreeNode();
                        RebuildChildCollectionTable();
                    }
                    NotifyRepositoryTreeChanged();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        public IRelayCommand CreateLeaveCommand =>
            _createLeaveCommand ??= _commandFactory.Create(
                obj =>
                {
                    if (_selectedRepositoryMember == null)
                        return;
                    if (_selectedRepositoryMember is ILeaveParent lp)
                    {
                        lp.CreateTreeLeave();
                        RebuildChildCollectionTable();
                    }
                    else if (_selectedRepositoryMember is TreeLeaveVM leave)
                    {
                        leave.Parent.CreateTreeLeave();
                    }
                    NotifyRepositoryTreeChanged();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        public IRelayCommand CreateAttributeCommand =>
            _createAttributeCommand ??= _commandFactory.Create(
                obj =>
                {
                    if (_selectedRepositoryMember == null)
                        return;
                    _selectedRepositoryMember.AddAttribute();
                    RebuildChildCollectionTable();

                    NotifyRepositoryTreeChanged();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        /// <summary>
        /// Подтверждает и применяет временно выбранный полиморфный родительский лист.
        /// </summary>
        public IAsyncRelayCommand ApplyPolymorphicParentCommand =>
            _applyPolymorphicParentCommand ??= _asyncCommandFactory.Create(
                async parameter =>
                {
                    if (parameter is LeavePolymorphismAttributeVM attribute)
                        await ApplyPolymorphicParentAsync(attribute);
                },
                parameter => CanModifyRepository()
                    && parameter is LeavePolymorphismAttributeVM);

        /// <summary>
        /// Создаёт отсутствующую цепочку полиморфных родителей без подтверждения.
        /// </summary>
        public IRelayCommand CreatePolymorphicParentCommand =>
            _createPolymorphicParentCommand ??= _commandFactory.Create(
                parameter =>
                {
                    if (parameter is LeavePolymorphismAttributeVM attribute)
                        CreatePolymorphicParent(attribute);
                },
                parameter => CanModifyRepository()
                    && parameter is LeavePolymorphismAttributeVM);

        public IAsyncRelayCommand SoftDeleteRepositoryMemberCommand =>
            _softDeleteRepositoryMemberCommand ??= _asyncCommandFactory.Create(
                async obj =>
                {
                    if (CanDeleteSelectedRepositoryMember()
                        && _selectedRepositoryMember?.Model is IContentModel c)
                    {
                        await SoftDeleteAsync(c);
                    }
                },
                ce =>
                {
                    return CanModifyRepository()
                        && CanDeleteSelectedRepositoryMember();
                });

        public IAsyncRelayCommand HardDeleteRepositoryMemberCommand =>
            _hardDeleteRepositoryMemberCommand ??= _asyncCommandFactory.Create(
                async obj =>
                {
                    if (CanDeleteSelectedRepositoryMember()
                        && _selectedRepositoryMember?.Model is IContentModel content)
                    {
                        await HardDeleteAsync(content);
                    }
                },
                ce => CanModifyRepository()
                    && CanDeleteSelectedRepositoryMember());

        public IAsyncRelayCommand SoftDeleteRepositoryMemberAttributeCommand =>
            _softDeleteRepositoryMemberAttributeCommand ??= _asyncCommandFactory.Create(
                async obj =>
                {
                    if (_selectedRepositoryMember?.SelectedAttributeVM?.Model is IContentModel c)
                    {
                        await SoftDeleteAsync(c);
                    }
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        /// <summary>
        /// Удаляет элемент после подтверждения сброса блокирующих связей.
        /// </summary>
        /// <param name="element">Удаляемый элемент.</param>
        /// <returns>Задача выполнения удаления.</returns>
        private async Task SoftDeleteAsync(IContentModel element)
        {
            if (element is not IMainEntityModel mainEntity)
                return;

            if (await ConfirmBlockingRelationsAsync(mainEntity) == false)
                return;

            if (_service.SoftDeleteShrubMember(element))
                NotifyRepositoryTreeChanged();
        }

        private async Task HardDeleteAsync(IContentModel element)
        {
            if (element is not IMainEntityModel mainEntity)
                return;

            var confirmed = await _dialogService.ConfirmAsync(
                $"Элемент '{mainEntity.Name}' [{mainEntity.Uuid}] будет помечен для безвозвратного удаления из хранилища данных при сохранении. Продолжить?",
                "Безвозвратное удаление");
            if (!confirmed || await ConfirmBlockingRelationsAsync(mainEntity) == false)
                return;

            if (_service.HardDeleteShrubMember(element))
                NotifyRepositoryTreeChanged();
        }

        private async Task<bool> ConfirmBlockingRelationsAsync(IMainEntityModel mainEntity)
        {
            var blockingRelations = await Task.Run(() => _relationsService
                .GetDirectRelations(PhiladelphusRepositoryVM.Model, mainEntity)
                .Where(x => x.BlocksSourceDeletion)
                .ToList());
            if (blockingRelations.Count == 0)
                return true;

            var rows = blockingRelations
                .GroupBy(x => x.Target.Uuid)
                .Select(group => new RelationDeletionWarningRow(
                    group.First().DisplayName,
                    string.Join(", ", group.Select(x => x.TypeDisplayName).Distinct())))
                .ToList();
            return await _relationDeletionConfirmationService.ConfirmAsync(
                $"'{mainEntity.Name}' [{mainEntity.Uuid}]",
                rows);
        }

        public IRelayCommand OpenModifyAttributesListWindowCommand =>
            _openModifyAttributesListWindowCommand ??= _commandFactory.Create(
                obj =>
                {
                    _windowService.Show(this);
                },
                ce =>
                {
                    return CanModifyRepository()
                        && (_selectedRepositoryMember?.SelectedAttributeVM?.IsCollectionValue ?? false);
                });

        public IRelayCommand AddAttributeValueCommand =>
            _addAttributeValueCommand ??= _commandFactory.Create(
                obj =>
                {
                    SelectedRepositoryMember.SelectedAttributeVM.AddSelectedValue();
                    RebuildChildCollectionTable();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        public IRelayCommand RemoveAttributeValueCommand =>
            _removeAttributeValueCommand ??= _commandFactory.Create(
                obj =>
                {
                    SelectedRepositoryMember.SelectedAttributeVM.RemoveSelectedValue();
                    RebuildChildCollectionTable();
                },
                ce =>
                {
                    return CanModifyRepository();
                });

        public IRelayCommand ProtectCommand =>
            _protectCommand ??= _commandFactory.Create(
                obj =>
                {
                },
                ce =>
                {
                    return false;
                });

        /// <summary>
        /// Перестраивает таблицу наследников, если после редактирования Sequence порядок строк стал устаревшим.
        /// </summary>
        public IRelayCommand RebuildChildCollectionTableIfOrderStaleCommand =>
            _rebuildChildCollectionTableIfOrderStaleCommand ??= _commandFactory.Create(
                _ =>
                {
                    if (_isChildCollectionTableOrderStale == false)
                    {
                        return;
                    }

                    RebuildChildCollectionTable();
                },
                _ =>
                {
                    return _isChildCollectionTableOrderStale && IsRepositoryLoading == false;
                });

        #endregion

        #region [ Methods ]

        internal bool LoadPhiladelphusRepository()
        {
            if (_isDisposed)
            {
                return false;
            }

            if (_repositoryLoadSemaphore.Wait(0) == false)
            {
                return false;
            }

            var loadVersion = BeginRepositoryLoad();

            try
            {
                if (CanApplyRepositoryLoad(loadVersion) == false)
                {
                    return false;
                }

                var newRepo = _service.GetShrubContent(_philadelphusRepositoryVM.Model);
                if (CanApplyRepositoryLoad(loadVersion) == false)
                {
                    return false;
                }

                UpdateLoadedPhiladelphusRepository(newRepo);
                return _philadelphusRepositoryVM.Childs != null;
            }
            finally
            {
                _repositoryLoadSemaphore.Release();
                CompleteRepositoryLoad(loadVersion);
            }
        }

        private async Task LoadPhiladelphusRepositoryOnStartupAsync()
        {
            await Task.Yield();

            try
            {
                await LoadPhiladelphusRepositoryAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка первичной загрузки содержимого репозитория.");
                _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                    $"Ошибка первичной загрузки содержимого репозитория. Подробности: {ex.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
            }
        }

        internal async Task<bool> LoadPhiladelphusRepositoryAsync()
        {
            if (_isDisposed)
            {
                return false;
            }

            var loadVersion = BeginRepositoryLoad();
            var lockTaken = false;

            try
            {
                await _repositoryLoadSemaphore.WaitAsync();
                lockTaken = true;

                if (CanApplyRepositoryLoad(loadVersion) == false)
                {
                    return false;
                }

                var newRepo = await _service.GetShrubContentAsync(_philadelphusRepositoryVM.Model);
                if (CanApplyRepositoryLoad(loadVersion) == false)
                {
                    return false;
                }

                UpdateLoadedPhiladelphusRepository(newRepo);
                return _philadelphusRepositoryVM.Childs != null;
            }
            finally
            {
                if (lockTaken)
                {
                    _repositoryLoadSemaphore.Release();
                }

                CompleteRepositoryLoad(loadVersion);
            }
        }

        private async Task ExecuteGetWorkAsync(object obj)
        {
            try
            {
                await LoadPhiladelphusRepositoryAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка обновления содержимого репозитория.");
                _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                    $"Ошибка обновления содержимого репозитория. Подробности: {ex.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
            }
        }

        private async Task ExecuteCreateWorkingTreeAsync(object obj)
        {
            var repository = _philadelphusRepositoryVM.Model;
            var dataStorage = await WorkingTreeDataStorageSelector.SelectAsync(
                repository.DefaultShrubMembersDataStorage,
                repository.DataStorages,
                _dataStorageSelectionDialogService);

            if (dataStorage == null)
                return;

            var tree = _service.CreateWorkingTree(repository, dataStorage);
            _service.CreateTreeRoot(tree);
            _philadelphusRepositoryVM.RebuildTreeItems();
            NotifyRepositoryTreeChanged();
        }

        private void UpdateLoadedPhiladelphusRepository(PhiladelphusRepositoryModel newRepo)
        {
            ArgumentNullException.ThrowIfNull(newRepo);
            ArgumentNullException.ThrowIfNull(newRepo.ContentShrub);
            ArgumentNullException.ThrowIfNull(newRepo.ContentShrub.ContentWorkingTrees);

            var selectedUuid = SelectedRepositoryMember?.Uuid;

            SelectedRepositoryMember = null;
            _philadelphusRepositoryVM.RebuildTreeItems();

            if (selectedUuid.HasValue)
            {
                SelectedRepositoryMember = FindRepositoryMemberByUuid(selectedUuid.Value);
            }

            NotifyRepositoryTreeChanged();
            NavigationVM.ResetHistory();

            // Значения одиночных атрибутов из БД намеренно не загружаются: ValueUuid хранится только
            // для SQL-запросов и отчетов. После перестроения модели представления восстанавливаем
            // runtime-значения вычислением ValueFormula.
            FormulaBarVM.RecalculateLoadedFormulas();
            RefreshLoadedLeavePolymorphismLinks();
        }

        /// <summary>
        /// Восстанавливает вычисляемые полиморфные связи после загрузки репозитория.
        /// </summary>
        /// <remarks>
        /// При загрузке связи только пересчитываются: значения атрибутов не перезаписываются,
        /// а подтверждение пользователя не запрашивается.
        /// </remarks>
        private void RefreshLoadedLeavePolymorphismLinks()
        {
            var polymorphicLeaves = _philadelphusRepositoryVM.Model.ContentShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentLeaves)
                .Where(x => x.Attributes.Any(attribute =>
                    attribute is LeavePolymorphismAttributeModel));

            _leavePolymorphismService.RefreshLinks(polymorphicLeaves);
        }

        /// <summary>
        /// Добавляет корневой узел рабочего дерева в отображаемый репозиторий.
        /// </summary>
        /// <param name="treeRoot">Корневой узел рабочего дерева.</param>
        /// <exception cref="ArgumentNullException">Если корневой узел не задан.</exception>
        public void AddTreeRoot(TreeRootModel treeRoot)
        {
            ArgumentNullException.ThrowIfNull(treeRoot);

            _philadelphusRepositoryVM.RebuildTreeItems();
            NotifyRepositoryTreeChanged();
        }

        /// <summary>
        /// Обновляет отображаемое дерево репозитория из текущей доменной модели.
        /// </summary>
        public void RefreshRepositoryView()
        {
            UpdateLoadedPhiladelphusRepository(_philadelphusRepositoryVM.Model);
        }

        private int BeginRepositoryLoad()
        {
            IsRepositoryLoading = true;
            return Interlocked.Increment(ref _repositoryLoadVersion);
        }

        private bool CanApplyRepositoryLoad(int loadVersion)
        {
            return _isDisposed == false
                && loadVersion == Volatile.Read(ref _repositoryLoadVersion);
        }

        private void CompleteRepositoryLoad(int loadVersion)
        {
            if (loadVersion == Volatile.Read(ref _repositoryLoadVersion))
            {
                IsRepositoryLoading = false;
            }
        }

        private void NotifyRepositoryTreeChanged()
        {
            OnPropertyChanged(nameof(PhiladelphusRepositoryVM));
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.Childs));
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.TreeItems));
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.ChildsCount));
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
            NotifyChildsPropertyChangedRecursive();
        }

        private void UpdateCurrentLeavesOwner(IMainEntityVM<IMainEntityModel>? selectedRepositoryMember)
        {
            CurrentLeavesOwner = selectedRepositoryMember switch
            {
                TreeNodeVM node => node,
                TreeLeaveVM leave => leave.Parent,
                _ => null,
            };
            if (selectedRepositoryMember is TreeLeaveVM l)
            {
                CurrentLeave = l;
            }   
            else
            {
                CurrentLeave = null;
            }
        }

        /// <summary>
        /// Синхронизирует выделение дерева с выбранным элементом репозитория.
        /// </summary>
        /// <param name="selectedRepositoryMember">Выбранный элемент репозитория.</param>
        private void SynchronizeTreeSelection(
            IMainEntityVM<IMainEntityModel>? selectedRepositoryMember)
        {
            var treeMember = selectedRepositoryMember is TreeLeaveVM
                ? null
                : selectedRepositoryMember;

            if (ReferenceEquals(_selectedRepositoryTreeMember, treeMember))
                return;

            _selectedRepositoryTreeMember = treeMember;
            OnPropertyChanged(nameof(SelectedRepositoryTreeMember));
        }

        internal IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(Guid uuid)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            foreach (var shrub in _philadelphusRepositoryVM.TreeItems)
            {
                if (shrub.Uuid == uuid)
                {
                    return shrub;
                }

                var found = FindRepositoryMemberByUuid(shrub, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            foreach (var root in _philadelphusRepositoryVM.Childs)
            {
                var found = FindRepositoryMemberByUuid(root, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(ShrubVM shrub, Guid uuid)
        {
            ArgumentNullException.ThrowIfNull(shrub);
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            foreach (var workingTree in shrub.WorkingTrees)
            {
                var found = FindRepositoryMemberByUuid(workingTree, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(WorkingTreeVM workingTree, Guid uuid)
        {
            ArgumentNullException.ThrowIfNull(workingTree);
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            if (workingTree.Uuid == uuid)
            {
                return workingTree;
            }

            foreach (var root in workingTree.Roots)
            {
                var found = FindRepositoryMemberByUuid(root, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(TreeRootVM root, Guid uuid)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            if (root.Uuid == uuid)
            {
                return root;
            }

            foreach (var node in root.ChildNodes)
            {
                var found = FindRepositoryMemberByUuid(node, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(TreeNodeVM node, Guid uuid)
        {
            ArgumentNullException.ThrowIfNull(node);
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            if (node.Uuid == uuid)
            {
                return node;
            }

            foreach (var childNode in node.ChildNodes)
            {
                var found = FindRepositoryMemberByUuid(childNode, uuid);
                if (found != null)
                {
                    return found;
                }
            }

            foreach (var leave in node.ChildLeaves)
            {
                if (leave.Uuid == uuid)
                {
                    return leave;
                }
            }

            return null;
        }

        private bool CanModifyRepository()
        {
            return IsRepositoryLoading == false;
        }

        private bool CanDeleteSelectedRepositoryMember()
        {
            return _selectedRepositoryMember
                is WorkingTreeVM
                or TreeRootVM
                or TreeNodeVM
                or TreeLeaveVM;
        }

        /// <summary>
        /// Перестраивает таблицу наследников для текущего выбранного элемента.
        /// </summary>
        /// <remarks>
        /// Колонки строятся по текущему элементу и его видимым для наследников атрибутам,
        /// строки - по рекурсивному списку наследников. После перестроения флаг устаревшего порядка сбрасывается.
        /// </remarks>
        internal void RebuildChildCollectionTable()
        {
            var children = GetSelectedRepositoryMemberChildren();

            _childCollectionTableColumns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                _selectedRepositoryMember?.Model as IAttributeOwnerModel,
                children);

            _childCollectionTableRows = ChildCollectionTableBuilder.buildChildCollectionTableRows(
                children,
                _childCollectionTableColumns,
                OnChildCollectionTableCellChanged);

            OnPropertyChanged(nameof(ChildCollectionTableColumns));
            OnPropertyChanged(nameof(ChildCollectionTableRows));
            _isChildCollectionTableOrderStale = false;
            _rebuildChildCollectionTableIfOrderStaleCommand?.RaiseCanExecuteChanged();
            _openModifyAttributesListWindowCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Синхронизирует остальные части интерфейса после редактирования ячейки таблицы наследников.
        /// </summary>
        /// <remarks>
        /// Для Sequence пересортировка откладывается до потери фокуса таблицей, чтобы строка не прыгала
        /// во время редактирования. Остальные изменения сразу пробрасывают уведомления в соответствующую VM.
        /// </remarks>
        private async void OnChildCollectionTableCellChanged(Guid sourceUuid, string columnKey)
        {
            if (sourceUuid == Guid.Empty || string.IsNullOrWhiteSpace(columnKey))
            {
                return;
            }

            var target = FindRepositoryMemberByUuid(sourceUuid);
            if (target is not ViewModelBase targetVM)
            {
                return;
            }

            targetVM.OnPropertyChanged(columnKey);
            targetVM.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.State));
            OnPropertyChanged(nameof(PropertyList));

            if (target.Model is IAttributeOwnerModel attributeOwner
                && attributeOwner.Attributes?.Any(x => string.Equals(x.Name, columnKey, StringComparison.Ordinal)) == true)
            {
                var changedAttribute = attributeOwner.Attributes.First(x => string.Equals(x.Name, columnKey, StringComparison.Ordinal));
                await FormulaBarVM.NotifyAttributeValueChangedAsync(changedAttribute);
                targetVM.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.AttributesVMs));
            }

            if (columnKey == nameof(IChildrenModel.SequencePath))
            {
                _isChildCollectionTableOrderStale = true;
                _rebuildChildCollectionTableIfOrderStaleCommand?.RaiseCanExecuteChanged();
                NotifyChildParentCollectionChanged(target);
            }
        }

        /// <summary>
        /// Уведомляет родительскую VM ребенка, что состав или порядок ее коллекций мог измениться.
        /// </summary>
        private void NotifyChildParentCollectionChanged(IMainEntityVM<IMainEntityModel> child)
        {
            if (child.Model is not IChildrenModel childModel)
            {
                return;
            }

            if (childModel.Parent is not IMainEntityModel parentModel)
            {
                return;
            }

            var parent = FindRepositoryMemberByUuid(parentModel.Uuid);
            switch (parent)
            {
                case TreeRootVM root:
                    root.OnPropertyChanged(nameof(TreeRootVM.ChildNodes));
                    break;
                case TreeNodeVM node:
                    node.OnPropertyChanged(nameof(TreeNodeVM.ChildNodes));
                    node.OnPropertyChanged(nameof(TreeNodeVM.ChildLeaves));
                    break;
            }
        }

        /// <summary>
        /// Возвращает рекурсивный список наследников выбранного элемента для отображения в таблице.
        /// </summary>
        private IReadOnlyList<IChildrenModel> GetSelectedRepositoryMemberChildren()
        {
            return ChildCollectionTableBuilder.buildChildCollectionTableChildren(
                _selectedRepositoryMember?.Model as IParentModel);
        }

        /// <summary>
        /// Проверяет доступность текущего репозитория Чубушника.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CheckPhiladelphusRepositoryAvailability()
        {
            if (_philadelphusRepositoryVM.Model == null)
                return false;
            return _philadelphusRepositoryVM.Model.OwnDataStorage.IsAvailable;
        }

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
            foreach (var item in _philadelphusRepositoryVM.TreeItems)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }

            foreach (var item in _philadelphusRepositoryVM.Childs)
            {
                item.NotifyChildsPropertyChangedRecursive();
            }
        }

        

        private bool UpdateChildsCollection(ViewModelBase parent)    //TODO: Переделать временный костыль
        {
            ArgumentNullException.ThrowIfNull(parent);

            if (parent is PhiladelphusRepositoryVM repository)
            {
                for (int i = repository.TreeItems.Count - 1; i >= 0; i--)
                {
                    UpdateChildsCollection(repository.TreeItems[i]);
                }

                for (int i = repository.Childs.Count - 1; i >= 0; i--)
                {
                    if (repository.Childs[i].State == State.ForHardDelete
                    || repository.Childs[i].State == State.ForSoftDelete
                    || repository.Childs[i].State == State.SoftDeleted)
                    {
                        repository.Childs.Remove(repository.Childs[i]);
                    }
                    else
                        UpdateChildsCollection(repository.Childs[i]);
                }
            }
            if (parent is ShrubVM shrub)
            {
                for (int i = shrub.WorkingTrees.Count - 1; i >= 0; i--)
                {
                    if (shrub.WorkingTrees[i].State == State.ForHardDelete
                    || shrub.WorkingTrees[i].State == State.ForSoftDelete
                    || shrub.WorkingTrees[i].State == State.SoftDeleted)
                    {
                        shrub.WorkingTrees.Remove(shrub.WorkingTrees[i]);
                    }
                    else
                        UpdateChildsCollection(shrub.WorkingTrees[i]);
                }
            }
            if (parent is WorkingTreeVM workingTree)
            {
                for (int i = workingTree.Roots.Count - 1; i >= 0; i--)
                {
                    if (workingTree.Roots[i].State == State.ForHardDelete
                    || workingTree.Roots[i].State == State.ForSoftDelete
                    || workingTree.Roots[i].State == State.SoftDeleted)
                    {
                        workingTree.Roots.Remove(workingTree.Roots[i]);
                    }
                    else
                        UpdateChildsCollection(workingTree.Roots[i]);
                }
            }
            if (parent is TreeRootVM root)
            {
                for (int i = root.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (root.ChildNodes[i].State == State.ForHardDelete
                    || root.ChildNodes[i].State == State.ForSoftDelete
                    || root.ChildNodes[i].State == State.SoftDeleted)
                    {
                        root.ChildNodes.Remove(root.ChildNodes[i]);
                    }
                    else
                        UpdateChildsCollection(root.ChildNodes[i]);
                }
            }
            if (parent is TreeNodeVM node)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (node.ChildNodes[i].State == State.ForHardDelete
                    || node.ChildNodes[i].State == State.ForSoftDelete
                    || node.ChildNodes[i].State == State.SoftDeleted)
                    {
                        node.ChildNodes.Remove(node.ChildNodes[i]);
                    }
                    else
                        UpdateChildsCollection(node.ChildNodes[i]);
                }
                for (int i = node.ChildLeaves.Count - 1; i >= 0; i--)
                {
                    if (node.ChildLeaves[i].State == State.ForHardDelete
                    || node.ChildLeaves[i].State == State.ForSoftDelete
                    || node.ChildLeaves[i].State == State.SoftDeleted)
                    {
                        node.ChildLeaves.Remove(node.ChildLeaves[i]);
                    }
                    else
                        UpdateChildsCollection(node.ChildLeaves[i]);
                }
            }
            return true;
        }

        private void RaiseRepositoryCommandsCanExecuteChanged()
        {
            _getWorkCommand?.RaiseCanExecuteChanged();
            _saveCommand?.RaiseCanExecuteChanged();
            _createWorkingTreeCommand?.RaiseCanExecuteChanged();
            _createNodeCommand?.RaiseCanExecuteChanged();
            _createLeaveCommand?.RaiseCanExecuteChanged();
            _createAttributeCommand?.RaiseCanExecuteChanged();
            _softDeleteRepositoryMemberCommand?.RaiseCanExecuteChanged();
            _softDeleteRepositoryMemberAttributeCommand?.RaiseCanExecuteChanged();
            _openModifyAttributesListWindowCommand?.RaiseCanExecuteChanged();
            _addAttributeValueCommand?.RaiseCanExecuteChanged();
            _removeAttributeValueCommand?.RaiseCanExecuteChanged();
            _rebuildChildCollectionTableIfOrderStaleCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Освобождает ресурсы обозревателя репозитория.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Interlocked.Increment(ref _repositoryLoadVersion);
        }

        internal void NotifyFormulaPropertyListChanged()
        {
            OnPropertyChanged(nameof(PropertyList));
        }

        internal void NotifyRepositoryStateChanged()
        {
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
        }

        /// <summary>
        /// Завершает ручное заполнение единым пересчётом формул и полиморфного каскада.
        /// </summary>
        private async Task ApplyPolymorphicParentAsync(
            LeavePolymorphismAttributeVM attribute)
        {
            var recipientLeave = attribute.RecipientLeave;
            var parentLeave = attribute.SelectedCandidate;
            if (recipientLeave == null || parentLeave == null)
                return;

            var fillResult = await _leavePolymorphismChangeCoordinator
                .FillFromParentAsync(recipientLeave, parentLeave);
            attribute.NotifyResolutionChanged();
            if (fillResult.Applied == false)
                return;

            var changeResult = new LeavePolymorphismChangeResult(false, []);
            if (fillResult.ChangedAttributes.Count > 0)
            {
                // Массовый пересчёт выполняется один раз для всех заполненных атрибутов,
                // после чего каскад видит окончательную сигнатуру листа.
                FormulaBarVM.RecalculateLoadedFormulas();
                changeResult = await _leavePolymorphismChangeCoordinator
                    .HandleChangedLeaveAsync(recipientLeave);
                if (changeResult.CascadeProcessed)
                    FormulaBarVM.RecalculateLoadedFormulas();
            }

            RefreshLeavePolymorphismView(changeResult);
        }

        /// <summary>
        /// Создаёт цепочку родителей и синхронизирует дерево, формулы и строку атрибута.
        /// </summary>
        private void CreatePolymorphicParent(LeavePolymorphismAttributeVM attribute)
        {
            if (attribute.RecipientLeave == null || attribute.CanCreateParent == false)
                return;

            var result = _leavePolymorphismChangeCoordinator
                .CreateParentChain(attribute.RecipientLeave);
            if (result.CreatedLeaves.Count > 0)
                FormulaBarVM.RecalculateLoadedFormulas();
            RefreshLeavePolymorphismView(result);
        }

        /// <summary>
        /// Передаёт строкам runtime-атрибутов команду автоматической обработки выбора.
        /// </summary>
        private void ConfigureLeavePolymorphismCommands(
            IMainEntityVM<IMainEntityModel>? repositoryMember)
        {
            foreach (var attribute in repositoryMember?.AttributesVMs ?? [])
            {
                attribute.LeavePolymorphism?
                    .SetParentSelectionCommand(ApplyPolymorphicParentCommand);
            }
        }

        /// <summary>
        /// Обновляет runtime-связи и элементы интерфейса после интерактивной операции.
        /// </summary>
        /// <param name="result">Результат обработки изменённого листа.</param>
        internal void RefreshLeavePolymorphismView(LeavePolymorphismChangeResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.CreatedLeaves.Count > 0)
            {
                var selectedUuid = SelectedRepositoryMember?.Uuid;
                _philadelphusRepositoryVM.RebuildTreeItems();
                SelectedRepositoryMember = selectedUuid.HasValue
                    ? FindRepositoryMemberByUuid(selectedUuid.Value)
                    : null;
            }

            if (result.CascadeProcessed)
            {
                RefreshLoadedLeavePolymorphismLinks();
                NotifyRepositoryTreeChanged();
            }

            foreach (var attribute in SelectedRepositoryMember?.AttributesVMs ?? [])
            {
                attribute.LeavePolymorphism?
                    .SetParentSelectionCommand(ApplyPolymorphicParentCommand);
                attribute.LeavePolymorphism?.NotifyResolutionChanged();
                attribute.OnPropertyChanged(nameof(ElementAttributeVM.DisplayedValueText));
                attribute.OnPropertyChanged(nameof(ElementAttributeVM.FormulaValueText));
            }

            RebuildChildCollectionTable();
            NotifyFormulaPropertyListChanged();
            NotifyRepositoryStateChanged();
        }

        #endregion

    }
}
