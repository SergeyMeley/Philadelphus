using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class DataStorageVM : ViewModelBase
    {
        private IDataStorageModel? _dataStorage;
        public IDataStorageModel? DataStorage { get => _dataStorage; set => _dataStorage = value; }
        public bool? IsAvailable { get => DataStorage.IsAvailable; }
        public DateTime? LastCheckTime { get => DataStorage.LastCheckTime; }
        public DataStorageVM(IDataStorageModel dataStorage)
        {
            _dataStorage = dataStorage;
            StartCheckingStorage();
        }
        private void StartCheckingStorage()
        {
            _dataStorage.StartAvailableAutoChecking();
            System.Timers.Timer timer = new System.Timers.Timer(100);
            timer.Elapsed += CheckStorage;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private void CheckStorage()
        {
            OnPropertyChanged(nameof(DataStorage));
            OnPropertyChanged(nameof(DataStorage.IsAvailable));
            OnPropertyChanged(nameof(DataStorage.LastCheckTime));
        }
        private void CheckStorage(Object source, ElapsedEventArgs e)
        {
            CheckStorage();
        }
    }
}
