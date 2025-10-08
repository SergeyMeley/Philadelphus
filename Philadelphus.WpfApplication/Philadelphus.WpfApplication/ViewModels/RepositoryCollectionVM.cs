using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.Models.Entities.Enums;
using Philadelphus.WpfApplication.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCollectionVM : ViewModelBase
    {
        private DataTreeProcessingService _dataTreeProcessingService = new DataTreeProcessingService();

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
        public RepositoryCollectionVM(DataStoragesSettingsVM dataStoragesSettings)
        {
            _dataStoragesSettingsVM = dataStoragesSettings;
            InitRepositoryCollection();
            PropertyGridRepresentation = PropertyGridRepresentations.DataGrid;
        }
        private static RepositoryExplorerVM _currentRepositoryExplorerVM;
        public  RepositoryExplorerVM CurrentRepositoryExplorerVM 
        { 
            get 
            { 
                return _currentRepositoryExplorerVM; 
            }
            set
            {
                _currentRepositoryExplorerVM = value;
                OnPropertyChanged(nameof(CurrentRepositoryExplorerVM));
                OnPropertyChanged(nameof(PropertyList));
            }
        }

        private ObservableCollection<RepositoryExplorerVM> _treeRepositoriesVMs = new ObservableCollection<RepositoryExplorerVM>();
        public ObservableCollection<RepositoryExplorerVM> TreeRepositoriesVMs
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
                _propertyGridRepresentation = (PropertyGridRepresentations)value;
                OnPropertyChanged(nameof(PropertyGridRepresentation));
            }
        }
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_currentRepositoryExplorerVM == null)
                    return null;
                return PropertyGridHelper.GetProperties(_currentRepositoryExplorerVM.TreeRepository);
            }
        }
        //TODO: Перенести в ApplicationVM
        public RelayCommand OpenRepositoryCollectionSettingsWindow
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = new RepositorySettingsWindow() { DataContext = this }; 
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
                    var service = new DataTreeProcessingService();
                    service.AddExistTreeRepository(new DirectoryInfo(""));
                });
            }
        }

        public RelayCommand CreateNewRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeProcessingService();
                    var builder = new DataStorageBuilder();
                    var repository = service.CreateNewTreeRepository(builder.Build());
                    var repositoryExplorerViewModel = new RepositoryExplorerVM(repository);
                    TreeRepositoriesVMs.Add(repositoryExplorerViewModel);
                     
                });
            }
        }
        private bool InitRepositoryCollection()
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage);
            var repositories = _dataTreeProcessingService.LoadRepositories(storages);
            foreach (var item in repositories)
            {
                _treeRepositoriesVMs.Add(new RepositoryExplorerVM(item));
            }
            return true;
        }

    }
}
