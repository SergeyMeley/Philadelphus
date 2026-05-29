using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Models.Tables;
using Philadelphus.Presentation.Wpf.UI.Services.Tables;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using Serilog;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления обозревателя репозитория Чубушника.
    /// </summary>
    public  class RepositoryExplorerControlVM : ControlBaseVM, IDisposable
    {
        #region [ Props ]

        private bool _isDisposed;

        private readonly IPhiladelphusRepositoryService _service;
        private readonly SemaphoreSlim _repositoryLoadSemaphore = new SemaphoreSlim(1, 1);
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
       
        private PhiladelphusRepositoryVM _philadelphusRepositoryVM;     // TODO: Тех. долг. Вернуть readonly
        private int _repositoryLoadVersion;

        private IMainEntityVM<IMainEntityModel>? _selectedRepositoryMember;
        private IReadOnlyList<ChildCollectionTableColumn> _childCollectionTableColumns = Array.Empty<ChildCollectionTableColumn>();
        private IReadOnlyList<ChildCollectionTableRow> _childCollectionTableRows = Array.Empty<ChildCollectionTableRow>();

        // Sequence редактируется прямо в таблице. Пересортировку откладываем до ухода фокуса,
        // чтобы строка не меняла позицию во время ввода значения.
        private bool _isChildCollectionTableOrderStale;

        public PhiladelphusRepositoryVM PhiladelphusRepositoryVM 
        { 
            get 
            { 
                return _philadelphusRepositoryVM; 
            }
        }

        public Visibility SystemBaseLeaveControlVisibility
        {
            get
            {
                if (SelectedRepositoryMember?.Model is SystemBaseTreeLeaveModel)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ParentControlVisibility
        {
            get
            {
                if (SelectedRepositoryMember is TreeRootVM 
                    || SelectedRepositoryMember is TreeNodeVM)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

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
                RebuildChildCollectionTable();
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(SelectedRepositoryMember));
                OnPropertyChanged(nameof(SystemBaseLeaveControlVisibility));
                OnPropertyChanged(nameof(ParentControlVisibility));
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

        private ExtensionsControlVM _extensionsControlVM;

        /// <summary>
        /// Модель представления панели расширений текущего обозревателя.
        /// </summary>
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }

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
                OnPropertyChanged(nameof(RepositoryLoadingVisibility));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Указывает, доступно ли содержимое репозитория для взаимодействия.
        /// </summary>
        public bool IsRepositoryContentEnabled => IsRepositoryLoading == false;

        /// <summary>
        /// Видимость индикатора загрузки репозитория.
        /// </summary>
        public Visibility RepositoryLoadingVisibility => IsRepositoryLoading
            ? Visibility.Visible
            : Visibility.Collapsed;

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
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RepositoryExplorerControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            IPhiladelphusRepositoryService service,
            IExtensionsControlVMFactory extensionVMFactory,
            ApplicationCommandsVM applicationCommandsVM,
            PhiladelphusRepositoryVM PhiladelphusRepositoryVM,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            bool loadOnStartup = true)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(extensionVMFactory);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryVM);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);

            _service = service;
            _extensionsControlVM = extensionVMFactory.Create(this);
            _philadelphusRepositoryVM = PhiladelphusRepositoryVM;
            _dataStoragesCollectionVM = dataStoragesCollectionVM;

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
        public AsyncRelayCommand GetWorkCommand
        {
            get
            {
                return new AsyncRelayCommand(ExecuteGetWorkAsync, _ => IsRepositoryLoading == false); ;
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var repo = _philadelphusRepositoryVM.Model;
                    _service.SaveChanges(ref repo, SaveMode.WithContentAndMembers);
                    UpdateChildsCollection(_philadelphusRepositoryVM);   // TODO
                    OnPropertyChanged(nameof(State));
                    NotifyChildsPropertyChangedRecursive();
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }
        public RelayCommand CreateRootCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var tree = _service.CreateWorkingTree(_philadelphusRepositoryVM.Model, _philadelphusRepositoryVM.Model.OwnDataStorage);
                    var result = _service.CreateTreeRoot(tree);
                    _philadelphusRepositoryVM.Childs.Add(new TreeRootVM(result, _dataStoragesCollectionVM, _service));
                    NotifyRepositoryTreeChanged();
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }
        public RelayCommand CreateNodeCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember == null)
                        return;
                    if (_selectedRepositoryMember is INodeParent np)
                    {
                        np.CreateTreeNode();
                        RebuildChildCollectionTable();
                    }
                    _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }
        public RelayCommand CreateLeaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
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
                    _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }
        public RelayCommand CreateAttributeCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember == null)
                        return;
                    _selectedRepositoryMember.AddAttribute();
                    RebuildChildCollectionTable();
                    
                    _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }
        public RelayCommand SoftDeleteRepositoryMemberCommand
        {
            get 
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember.Model is IContentModel c)
                    {
                        _service.SoftDeleteShrubMember(c);
                        (_selectedRepositoryMember as ViewModelBase)?.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.State));
                        NotifyChildsPropertyChangedRecursive();
                    }
                },
                ce =>
                {
                    return CanModifyRepository()
                        && _selectedRepositoryMember?.Model is IContentModel;
                });
            }
        }
        public RelayCommand SoftDeleteRepositoryMemberAttributeCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_selectedRepositoryMember?.SelectedAttributeVM?.Model is IContentModel c)
                    {
                        _service.SoftDeleteShrubMember(c);
                        _selectedRepositoryMember.SelectedAttributeVM.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.State));
                    }
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }

        public RelayCommand OpenModifyAttributesListWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = _serviceProvider.GetRequiredService<AttributeValuesCollectionWindow>();
                    window.DataContext = this;
                    window.Show();
                },
                ce =>
                {
                    return CanModifyRepository()
                        && (_selectedRepositoryMember?.SelectedAttributeVM?.IsCollectionValue ?? false);
                });
            }
        }

        public RelayCommand AddAttributeValueCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SelectedRepositoryMember.SelectedAttributeVM.AddSelectedValue();
                    RebuildChildCollectionTable();
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }

        public RelayCommand RemoveAttributeValueCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SelectedRepositoryMember.SelectedAttributeVM.RemoveSelectedValue();
                    RebuildChildCollectionTable();
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }

        public RelayCommand ProtectCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                },
                ce =>
                {
                    return false;
                });
            }
        }

        /// <summary>
        /// Перестраивает таблицу наследников, если после редактирования Sequence порядок строк стал устаревшим.
        /// </summary>
        public RelayCommand RebuildChildCollectionTableIfOrderStaleCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    if (_isChildCollectionTableOrderStale == false)
                    {
                        return;
                    }

                    RebuildChildCollectionTable();
                },
                _ => _isChildCollectionTableOrderStale && IsRepositoryLoading == false);
            }
        }

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

        private void UpdateLoadedPhiladelphusRepository(PhiladelphusRepositoryModel newRepo)
        {
            ArgumentNullException.ThrowIfNull(newRepo);
            ArgumentNullException.ThrowIfNull(newRepo.ContentShrub);
            ArgumentNullException.ThrowIfNull(newRepo.ContentShrub.ContentWorkingTrees);

            var selectedUuid = SelectedRepositoryMember?.Uuid;

            SelectedRepositoryMember = null;
            _philadelphusRepositoryVM.Childs.Clear();
            foreach (var item in newRepo.ContentShrub.ContentWorkingTrees)
            {
                if (item.ContentRoot != null)
                {
                    _philadelphusRepositoryVM.Childs.Add(new TreeRootVM(item.ContentRoot, _dataStoragesCollectionVM, _service));
                }
            }

            if (selectedUuid.HasValue)
            {
                SelectedRepositoryMember = FindRepositoryMemberByUuid(selectedUuid.Value);
            }

            NotifyRepositoryTreeChanged();
        }

        /// <summary>
        /// Добавляет корневой узел рабочего дерева в отображаемый репозиторий.
        /// </summary>
        /// <param name="treeRoot">Корневой узел рабочего дерева.</param>
        /// <exception cref="ArgumentNullException">Если корневой узел не задан.</exception>
        public void AddTreeRoot(TreeRootModel treeRoot)
        {
            ArgumentNullException.ThrowIfNull(treeRoot);

            _philadelphusRepositoryVM.Childs.Add(new TreeRootVM(treeRoot, _dataStoragesCollectionVM, _service));
            NotifyRepositoryTreeChanged();
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
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.ChildsCount));
            _philadelphusRepositoryVM.OnPropertyChanged(nameof(PhiladelphusRepositoryVM.State));
        }

        private IMainEntityVM<IMainEntityModel>? FindRepositoryMemberByUuid(Guid uuid)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

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

        /// <summary>
        /// Перестраивает таблицу наследников для текущего выбранного элемента.
        /// </summary>
        /// <remarks>
        /// Колонки строятся по текущему элементу и его видимым для наследников атрибутам,
        /// строки - по рекурсивному списку наследников. После перестроения флаг устаревшего порядка сбрасывается.
        /// </remarks>
        private void RebuildChildCollectionTable()
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
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Синхронизирует остальные части интерфейса после редактирования ячейки таблицы наследников.
        /// </summary>
        /// <remarks>
        /// Для Sequence пересортировка откладывается до потери фокуса таблицей, чтобы строка не прыгала
        /// во время редактирования. Остальные изменения сразу пробрасывают уведомления в соответствующую VM.
        /// </remarks>
        private void OnChildCollectionTableCellChanged(Guid sourceUuid, string columnKey)
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
                targetVM.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.AttributesVMs));
            }

            if (columnKey == nameof(ISequencableModel.Sequence))
            {
                _isChildCollectionTableOrderStale = true;
                CommandManager.InvalidateRequerySuggested();
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

        #endregion
    }
}
