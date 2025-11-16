using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.WpfApplication.Models.StorageConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Philadelphus.WpfApplication.ViewModels.InfrastructureVMs
{
    public class DataStoragesSettingsVM : ViewModelBase
    {
        private ObservableCollection<DataStorageVM>? _dataStorageVMs = new ObservableCollection<DataStorageVM>();
        public ObservableCollection<DataStorageVM>? DataStorageVMs 
        { 
            get
            {
                return _dataStorageVMs;
            }
            set
            {
                _dataStorageVMs = value;
                OnPropertyChanged(nameof(DataStorageVMs));
            }
        }

        private DataStorageVM _selectedDataStorageVM;
        public DataStorageVM SelectedDataStorageVM 
        {
            get
            {
                return _selectedDataStorageVM;
            }
            set
            {
                _selectedDataStorageVM = value;
                OnPropertyChanged(nameof(SelectedDataStorageVM));
            }
        }

        public DataStoragesSettingsVM()
        {
            InitDataStorages();
        }
        private bool InitDataStorages()
        {
            var service = new StorageConfigService();
            service.LoadConfig();
            var models = service.GetAllStorageModels();
            foreach (var model in models)
            {
                if (model != null)
                {
                    if (_dataStorageVMs.FirstOrDefault(x => x.Model.Guid == model.Guid) == null)
                    {
                        _dataStorageVMs.Add(new DataStorageVM(model));
                    }
                }
            }
            return true;
        }
    }
}
