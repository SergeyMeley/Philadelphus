using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.OtherEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using PropertyTools.Wpf;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public  class RepositoryExplorerControlVM : ControlBaseVM, IDisposable
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;
        private readonly SemaphoreSlim _repositoryLoadSemaphore = new SemaphoreSlim(1, 1);
        private PhiladelphusRepositoryVM _philadelphusRepositoryVM;     // TODO: Тех. долг. Вернуть readonly
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
        private int _repositoryLoadVersion;
        private bool _isDisposed;
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

        public string CurentRepositoryName { get => _philadelphusRepositoryVM.Name; }

        //public List<IMainEntityModel> ElementsCollection { get; internal set; } = new List<IMainEntityModel>();

        private IMainEntityVM<IMainEntityModel>? _selectedRepositoryMember;
        public IMainEntityVM<IMainEntityModel>? SelectedRepositoryMember
        {
            get
            {
                return _selectedRepositoryMember;
            }
            set
            {
                _selectedRepositoryMember = value;
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

        private ExtensionsControlVM _extensionsControlVM;
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }

        public string CurrentProcess { get; private set; }
        public string CurrentProgress { get; private set; }

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

        public bool IsRepositoryContentEnabled => IsRepositoryLoading == false;

        public Visibility RepositoryLoadingVisibility => IsRepositoryLoading
            ? Visibility.Visible
            : Visibility.Collapsed;

        #endregion

        #region [ Construct ]

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
            DataStoragesCollectionVM dataStoragesCollectionVM)
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

            _ = LoadPhiladelphusRepositoryOnStartupAsync();

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

        public RelayCommand ExportToPhjsonCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var model = SelectedRepositoryMember.Model as TreeRootModel;
                    var json = JsonImportExportHelper.GetJson(model.OwningWorkingTree);

                    Clipboard.SetText(json);

                    var path = string.Empty;

                    var dialog = new OpenFolderDialog
                    {
                        Title = "Выберите директорию",
                        Multiselect = false,
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        path = dialog.FolderName;
                    }

                    string file = System.IO.Path.Combine(path, $"philadelphus-export-{Guid.CreateVersion7()}.phjson");
                    File.WriteAllText(file, json, Encoding.UTF8);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = file,
                        UseShellExecute = true
                    });
                },
                ce =>
                {
                    return CanModifyRepository() && SelectedRepositoryMember is TreeRootVM;
                });
            }
        }

        public RelayCommand ImportFromPhjsonCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Выберите файл",
                        Multiselect = false,
                        Filter = "PHJSON файлы (*.phjson)|*.phjson",
                        FilterIndex = 1,
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        var file = dialog.FileName;

                        _ = Task.Run(() =>
                        {
                            var json = File.ReadAllText(file);

                            JsonImportExportHelper.ParseJson(json, _service, PhiladelphusRepositoryVM.Model, OnProcessChanged, OnProgressChanged);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var root = PhiladelphusRepositoryVM?.Model?.ContentShrub?.ContentWorkingTrees?.Last()?.ContentRoot;
                                var rootVM = new TreeRootVM(root, _dataStoragesCollectionVM, _service);

                                PhiladelphusRepositoryVM.Childs.Add(rootVM);
                            });
                        });
                    }
                },
                ce =>
                {
                    return CanModifyRepository();
                });
            }
        }

        public RelayCommand ConvertXlsxToPhjsonCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = _serviceProvider.GetRequiredService<ImportFromExcelWindow>();
                    window.ShowDialog();
                },
                ce =>
                {
                    return CanModifyRepository();
                });
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

        private void OnProcessChanged(string currentProcess)
        {
            ArgumentNullException.ThrowIfNull(currentProcess);

            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentProcess = currentProcess;
                OnPropertyChanged(nameof(CurrentProcess));
            });
        }

        private void OnProgressChanged(int currentNumber, int totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(currentNumber);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalCount);

            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentProgress = $"{currentNumber} / {totalCount} ({Math.Round(((double)currentNumber / (double)totalCount * 100), 1)} %)";
                OnPropertyChanged(nameof(CurrentProgress));
            });
        }

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
