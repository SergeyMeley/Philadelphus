using Microsoft.Data.Sqlite;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления редактируемых параметров строки подключения SQLite.
    /// </summary>
    public class SqliteConnectionStringVM : ViewModelBase
    {
        private string _dataSource = string.Empty;

        /// <summary>
        /// Путь или другое имя источника данных SQLite.
        /// </summary>
        public string DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (_dataSource == value)
                    return;

                _dataSource = value;
                OnPropertyChanged(nameof(DataSource));
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        /// <summary>
        /// Строка подключения, построенная штатным SQLite builder.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return new SqliteConnectionStringBuilder
                {
                    DataSource = DataSource
                }.ConnectionString;
            }
            set
            {
                var builder = new SqliteConnectionStringBuilder(value ?? string.Empty);
                DataSource = builder.DataSource;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public SqliteConnectionStringVM(string connectionString = "")
        {
            ConnectionString = connectionString;
        }
    }
}
