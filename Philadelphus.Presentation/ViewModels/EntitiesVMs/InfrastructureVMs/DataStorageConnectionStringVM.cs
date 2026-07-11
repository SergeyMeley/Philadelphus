using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using System.ComponentModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления строки подключения для группы сущностей.
    /// </summary>
    public class DataStorageConnectionStringVM : ViewModelBase
    {
        private string _connectionString;
        private bool _isUpdatingSqliteEditor;

        public InfrastructureEntityGroups EntityGroup { get; }

        /// <summary>
        /// Редактор SQLite, если он запрошен владельцем строки.
        /// </summary>
        public SqliteConnectionStringVM? SqliteEditor { get; }

        /// <summary>
        /// Отображаемое наименование группы сущностей.
        /// </summary>
        public string EntityGroupDisplayName
        {
            get
            {
                return EntityGroup.GetDisplayName();
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
                if (_connectionString == value)
                    return;

                _connectionString = value;
                OnPropertyChanged(nameof(ConnectionString));

                if (SqliteEditor != null && _isUpdatingSqliteEditor == false)
                    SqliteEditor.ConnectionString = value;
            }
        }

        public DataStorageConnectionStringVM(
            InfrastructureEntityGroups entityGroup,
            string connectionString,
            bool createSqliteEditor = false)
        {
            EntityGroup = entityGroup;
            _connectionString = connectionString;
            if (createSqliteEditor)
            {
                SqliteEditor = new SqliteConnectionStringVM(connectionString);
                SqliteEditor.PropertyChanged += SqliteEditorPropertyChanged;
            }
        }

        private void SqliteEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SqliteConnectionStringVM.ConnectionString))
                return;

            _isUpdatingSqliteEditor = true;
            ConnectionString = SqliteEditor!.ConnectionString;
            _isUpdatingSqliteEditor = false;
        }
    }
}
