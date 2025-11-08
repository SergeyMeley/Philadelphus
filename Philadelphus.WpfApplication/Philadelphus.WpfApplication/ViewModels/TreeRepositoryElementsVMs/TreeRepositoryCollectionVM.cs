using MongoDB.Driver.Linq;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.WpfApplication.Models.Entities.Enums;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
{
    public class TreeRepositoryCollectionVM : ViewModelBase
    {
        private TreeRepositoryCollectionService _service;

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
        public TreeRepositoryCollectionVM(TreeRepositoryCollectionService service, DataStoragesSettingsVM dataStoragesSettings)
        {
            _service = service;
            _dataStoragesSettingsVM = dataStoragesSettings;
            InitRepositoriesVMsCollection();
            PropertyGridRepresentation = PropertyGridRepresentations.DataGrid;
        }
        private static TreeRepositoryVM _currentRepositoryExplorerVM;
        public TreeRepositoryVM CurrentRepositoryExplorerVM
        {
            get
            {
                return _currentRepositoryExplorerVM;
            }
            set
            {
                _currentRepositoryExplorerVM = null;
                _currentRepositoryExplorerVM = value;
                _currentRepositoryExplorerVM.LoadTreeRepository();
                OnPropertyChanged(nameof(CurrentRepositoryExplorerVM));
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(TreeRepositoriesVMs));
            }
        }

        private ObservableCollection<TreeRepositoryVM> _treeRepositoriesVMs = new ObservableCollection<TreeRepositoryVM>();
        public ObservableCollection<TreeRepositoryVM> TreeRepositoriesVMs
        {
            get => _treeRepositoriesVMs;
            //private set
            //{
            //    _treeRepositoriesVMs = value.ToList();
            //    OnPropertyChanged(nameof(TreeRepositoriesVMs));
            //}
        }


        public List<string> PropertyGridRepresentationsCollection
        {
            get
            {
                var list = new List<string>();
                foreach (var item in Enum.GetNames(typeof(PropertyGridRepresentations)))
                {
                    list.Add(item);
                }
                return list;
            }
        }

        private PropertyGridRepresentations _propertyGridRepresentation;
        public PropertyGridRepresentations PropertyGridRepresentation
        {
            get
            {
                return _propertyGridRepresentation;
            }
            set
            {
                _propertyGridRepresentation = value;
                OnPropertyChanged(nameof(PropertyGridRepresentation));
            }
        }
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_currentRepositoryExplorerVM == null)
                    return null;
                //return PropertyGridHelper.GetProperties(_currentRepositoryExplorerVM.TreeRepository);
                return null;
            }
        }
        //TODO: Перенести в ApplicationVM
        public RelayCommand OpenRepositoryCollectionSettingsWindow
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = new RepositorySettingsWindow(this);
                    window.Show();
                });
            }
        }

        public RelayCommand AddExistRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _service.AddExistTreeRepository(new DirectoryInfo(""));
                });
            }
        }

        public RelayCommand CreateNewRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var builder = new DataStorageBuilder();
                    var repository = _service.CreateNewTreeRepository(builder.Build());
                    var repositoryExplorerViewModel = new TreeRepositoryVM(repository);
                    TreeRepositoriesVMs.Add(repositoryExplorerViewModel);
                     
                });
            }
        }
        private bool InitRepositoriesVMsCollection()
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage);
            var repositories = _service.GetTreeRepositoriesCollection(storages);
            if (repositories == null)
                return false;
            foreach (var item in repositories)
            {
                _treeRepositoriesVMs.Add(new TreeRepositoryVM(item));
            }
            return true;
        }
        internal bool CheckTreeRepositoryVMAvailable(Guid guid, out TreeRepositoryVM outTreeRepositoryVM)
        {
            outTreeRepositoryVM = TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == guid);
            if (outTreeRepositoryVM != null && outTreeRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            outTreeRepositoryVM = InitTreeRepositoryVM(guid);
            if (outTreeRepositoryVM != null && outTreeRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            return false;
        }
        private TreeRepositoryVM InitTreeRepositoryVM(Guid guid)
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage);
            var repositories = _service.GetTreeRepositoriesCollection(storages, new[] { guid });
            if (repositories == null)
                return null;
            var repository = repositories.FirstOrDefault(x => x.Guid == guid);
            if (repository == null)
                return null;
            var result = new TreeRepositoryVM(repository);
            _treeRepositoriesVMs.Add(result);
            return result;
        }
    }
}
