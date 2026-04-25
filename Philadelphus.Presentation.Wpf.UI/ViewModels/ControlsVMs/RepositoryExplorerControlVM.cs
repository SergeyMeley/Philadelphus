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
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using PropertyTools.Wpf;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public  class RepositoryExplorerControlVM : ControlBaseVM
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;
        private PhiladelphusRepositoryVM _philadelphusRepositoryVM;     // TODO: Тех. долг. Вернуть readonly
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
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
            _service = service;
            _extensionsControlVM = extensionVMFactory.Create(this);
            _philadelphusRepositoryVM = PhiladelphusRepositoryVM;
            _dataStoragesCollectionVM = dataStoragesCollectionVM;

            LoadPhiladelphusRepository();

            _notificationService.SendTextMessage<RepositoryExplorerControlVM>("Обозреватель репозитория. Начало инициализации расширений.", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectories);
            _notificationService.SendTextMessage<RepositoryExplorerControlVM>($"Обозреватель репозитория. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт.).", NotificationCriticalLevelModel.Info);
        }

        #endregion

        #region [Commands]
        public RelayCommand GetWorkCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    LoadPhiladelphusRepository();
                });
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
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.State));
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
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.State));
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
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.Childs));
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.State));
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
                    
                    OnPropertyChanged(nameof(_philadelphusRepositoryVM.State));
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
                        OnPropertyChanged(nameof(_selectedRepositoryMember.State));
                        NotifyChildsPropertyChangedRecursive();
                    }
                },
                ce =>
                {
                    return _selectedRepositoryMember?.Model is IContentModel;
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
                        OnPropertyChanged(nameof(_selectedRepositoryMember.SelectedAttributeVM.State));
                    }
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
                    return _selectedRepositoryMember?.SelectedAttributeVM?.IsCollectionValue ?? false;
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
                    return SelectedRepositoryMember is TreeRootVM;
                });
            }
        }

        public RelayCommand ImportFromPhjsonCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var path = string.Empty;

                    var dialog = new OpenFileDialog
                    {
                        Title = "Выберите файл",
                        Multiselect = false,
                        Filter = "PHJSON файлы (*.phjson)|*.phjson",
                        FilterIndex = 1,
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        string file = dialog.FileName;
                        var json = File.ReadAllText(file);

                        JsonImportExportHelper.ParseJson(json, _service, PhiladelphusRepositoryVM.Model);

                        var root = PhiladelphusRepositoryVM?.Model?.ContentShrub?.ContentWorkingTrees?.Last()?.ContentRoot;
                        var rootVM = new TreeRootVM(root, _dataStoragesCollectionVM, _service);
                        PhiladelphusRepositoryVM.Childs.Add(rootVM);
                    }
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
                });
            }
        }

        #endregion

        #region [ Methods ]

        internal bool LoadPhiladelphusRepository()
        {
            var newRepo = _service.GetShrubContent(_philadelphusRepositoryVM.Model);
            _philadelphusRepositoryVM.Childs.Clear();
            foreach (var item in newRepo.ContentShrub.ContentWorkingTrees)
            {
                if (item.ContentRoot != null)
                {
                    _philadelphusRepositoryVM.Childs.Add(new TreeRootVM(item.ContentRoot, _dataStoragesCollectionVM, _service));
                }
            }
            OnPropertyChanged(nameof(_philadelphusRepositoryVM));
            OnPropertyChanged(nameof(_philadelphusRepositoryVM.Childs));
            OnPropertyChanged(nameof(_philadelphusRepositoryVM.ChildsCount));
            return _philadelphusRepositoryVM.Childs != null;
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

        #endregion
    }
}
