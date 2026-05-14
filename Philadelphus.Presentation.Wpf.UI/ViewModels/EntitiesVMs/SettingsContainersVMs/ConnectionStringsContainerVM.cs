using Philadelphus.Core.Domain.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs
{
    /// <summary>
    /// Модель представления для контейнера строк подключения.
    /// </summary>
    public class ConnectionStringsContainerVM : ViewModelBase
    {
        private ConnectionStringsContainer _connectionStringsContainer;
        private string _providerName;
        private string _connectionString;
        private bool _forDelete;
      
        /// <summary>
        /// Выполняет операцию Uuid.
        /// </summary>
        /// <returns>Созданный объект.</returns>
        public Guid Uuid { get; internal set; } = Guid.CreateVersion7();
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
        public bool ForDelete 
        { 
            get
            {
                return _forDelete;
            }
            set
            {
                if (_forDelete != value)
                {
                    _forDelete = value;
                    OnPropertyChanged(nameof(ForDelete));
                }
            }
        }
      
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ConnectionStringsContainerVM" />.
        /// </summary>
        public ConnectionStringsContainerVM()
        {
            _connectionStringsContainer = new ConnectionStringsContainer()
            {
                StorageUuid = Guid.CreateVersion7(),
                ProviderName = string.Empty,
                ConnectionStrings = new()
            };
            OnPropertyChanged();
        }
    }
}
