using Philadelphus.Business.Entities.Infrastructure;
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
        public ObservableCollection<IDataStorageModel>? DataStorages { get; set; } = new ObservableCollection<IDataStorageModel>();
        public IDataStorageModel SelectedDataStorage {  get; set; }
    }
}
