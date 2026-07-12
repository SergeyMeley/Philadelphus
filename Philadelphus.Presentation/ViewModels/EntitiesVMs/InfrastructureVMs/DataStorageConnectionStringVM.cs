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

        public InfrastructureEntityGroups EntityGroup { get; }

        /// <summary>
        /// Редактор SQLite, если он запрошен владельцем строки.
        /// </summary>
        public SqliteConnectionStringVM? SqliteEditor { get; }
        public PostgreSqlConnectionStringVM? PostgreSqlEditor { get; }

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
            }
        }

        public DataStorageConnectionStringVM(
            InfrastructureEntityGroups entityGroup,
            string connectionString,
            bool createSqliteEditor = false,
            bool createPostgreSqlEditor = false)
        {
            EntityGroup = entityGroup;
            _connectionString = connectionString;
            if (createSqliteEditor)
            {
                SqliteEditor = new SqliteConnectionStringVM(connectionString);
                SqliteEditor.PropertyChanged += SqliteEditorPropertyChanged;
            }
            if (createPostgreSqlEditor)
            {
                PostgreSqlEditor = new PostgreSqlConnectionStringVM(connectionString);
                PostgreSqlEditor.PropertyChanged += PostgreSqlEditorPropertyChanged;
            }
        }

        private void PostgreSqlEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PostgreSqlConnectionStringVM.ConnectionString))
                return;
            ConnectionString = PostgreSqlEditor!.ConnectionString;
        }

        private void SqliteEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SqliteConnectionStringVM.ConnectionString))
                return;

            ConnectionString = SqliteEditor!.ConnectionString;
        }
    }
}
