using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.WpfApplication.Models.StorageConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class DataStoragesSettingsVM
    {
        private ObservableCollection<IDataStorageModel>? _dataStorages = new ObservableCollection<IDataStorageModel>();
        public ObservableCollection<IDataStorageModel>? DataStorages { get => _dataStorages; set => _dataStorages = value; }

        private IDataStorageModel _selectedDataStorage;
        public IDataStorageModel SelectedDataStorage { get => _selectedDataStorage; set => _selectedDataStorage = value; }
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
                    if (_dataStorages.FirstOrDefault(x => x.Guid == model.Guid) == null)
                    {
                        _dataStorages.Add(model);
                    }
                }
            }
            return true;
        }
    }
}
