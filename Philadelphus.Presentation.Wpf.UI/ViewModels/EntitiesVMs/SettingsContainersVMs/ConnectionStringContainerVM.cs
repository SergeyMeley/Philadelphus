using Philadelphus.Core.Domain.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs
{
    public class ConnectionStringContainerVM : ViewModelBase
    {
        private ConnectionStringContainer _connectionStringContainer;
        private string _connectionString;
        private string _providerName;
        public Guid Uuid { get; internal set; } = Guid.NewGuid();
        public string ProviderName 
        { 
            get
            {
                return _providerName; 
            }
            init
            {
                 _providerName = value;
                OnPropertyChanged(nameof(ProviderName));
            }
        }
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    OnPropertyChanged(nameof(ConnectionString));
                }
            }
        }
        public ConnectionStringContainerVM()
        {
            _connectionStringContainer = new ConnectionStringContainer()
            {
                Uuid = Guid.NewGuid(),
                ProviderName = string.Empty,
                ConnectionString = string.Empty
            };
            OnPropertyChanged();
        }
    }
}
