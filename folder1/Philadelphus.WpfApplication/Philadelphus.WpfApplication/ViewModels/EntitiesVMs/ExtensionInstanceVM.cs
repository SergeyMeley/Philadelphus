using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.WpfApplication.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.EntitiesVMs
{
    public class ExtensionInstanceVM : ViewModelBase
    {
        private readonly ExtensionInstance _extensionInstance;

        public ExtensionInstanceVM(ExtensionInstance extensionInstance)
        {
            _extensionInstance = extensionInstance;
            _extensionInstance.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }

        public string Name => _extensionInstance.Metadata.Name;
        public string Description => _extensionInstance.Metadata.Description;
        public string Version => _extensionInstance.Metadata.Version;

        public ExtensionState State => _extensionInstance.State;
        public bool CanExecute => _extensionInstance.LastCanExecuteResultModel.CanExecute;
        public string CanExecuteMessage => _extensionInstance.LastCanExecuteResultModel.Message;
        public object Window => _extensionInstance.Window;
        public object RibbonWidget => _extensionInstance.RibbonWidget;
        public object RepositoryExplorerWidget => _extensionInstance.RepositoryExplorerWidget;
        public bool IsWidgetsInitialized => _extensionInstance.IsWidgetInitialized;

        public ObservableCollection<OperationLog> OperationHistory => _extensionInstance.OperationHistory;

        public async Task StartAsync()
        {
            await _extensionInstance.StartAsync();
        }

        public async Task StopAsync()
        {
            await _extensionInstance.StopAsync();
        }

        public async Task ExecuteAsync(MainEntityBaseModel element)
        {
            await _extensionInstance.ExecuteAsync(element);
        }

        public async Task UpdateCanExecuteAsync(MainEntityBaseModel element)
        {
            await _extensionInstance.UpdateCanExecuteAsync(element);
        }

    }
}
