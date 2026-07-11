using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления строки подключения для группы сущностей.
    /// </summary>
    public class DataStorageConnectionStringVM : ViewModelBase
    {
        private string _connectionString;

        public InfrastructureEntityGroups EntityGroup { get; }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (_connectionString == value)
                    return;

                _connectionString = value;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public DataStorageConnectionStringVM(InfrastructureEntityGroups entityGroup, string connectionString)
        {
            EntityGroup = entityGroup;
            _connectionString = connectionString;
        }
    }
}
