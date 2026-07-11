using Microsoft.Data.Sqlite;
using Philadelphus.Presentation.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления редактируемых параметров строки подключения SQLite.
    /// </summary>
    public class SqliteConnectionStringVM : ViewModelBase
    {
        private string _dataSource = string.Empty;

        /// <summary>
        /// Дополнительные параметры строки подключения SQLite.
        /// </summary>
        public ObservableCollection<ConnectionStringParameterVM> AdditionalParameters { get; } = new();

        /// <summary>
        /// Путь к файлу базы данных SQLite, сохраняемый в параметре Data Source.
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
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = DataSource
                };
                foreach (var parameter in AdditionalParameters.Where(x => string.IsNullOrWhiteSpace(x.Key) == false))
                    builder[parameter.Key] = parameter.Value;

                return builder.ConnectionString;
            }
            set
            {
                var builder = new SqliteConnectionStringBuilder(value ?? string.Empty);
                DataSource = builder.DataSource;
                AdditionalParameters.Clear();
                foreach (KeyValuePair<string, object> parameter in builder)
                {
                    if (string.Equals(parameter.Key, "Data Source", StringComparison.OrdinalIgnoreCase))
                        continue;

                    AdditionalParameters.Add(new ConnectionStringParameterVM(
                        parameter.Key,
                        parameter.Value?.ToString() ?? string.Empty));
                }
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public SqliteConnectionStringVM(string connectionString = "")
        {
            AdditionalParameters.CollectionChanged += AdditionalParametersCollectionChanged;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Добавляет пустой дополнительный параметр.
        /// </summary>
        public RelayCommand AddAdditionalParameterCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    AdditionalParameters.Add(new ConnectionStringParameterVM(string.Empty, string.Empty));
                });
            }
        }

        /// <summary>
        /// Удаляет указанный дополнительный параметр.
        /// </summary>
        public RelayCommand RemoveAdditionalParameterCommand
        {
            get
            {
                return new RelayCommand(
                    parameter =>
                    {
                        AdditionalParameters.Remove((ConnectionStringParameterVM)parameter);
                    },
                    parameter => parameter is ConnectionStringParameterVM vm
                        && AdditionalParameters.Contains(vm));
            }
        }

        private void AdditionalParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ConnectionStringParameterVM parameter in e.OldItems)
                    parameter.PropertyChanged -= AdditionalParameterPropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (ConnectionStringParameterVM parameter in e.NewItems)
                    parameter.PropertyChanged += AdditionalParameterPropertyChanged;
            }

            OnPropertyChanged(nameof(ConnectionString));
        }

        private void AdditionalParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectionString));
        }
    }
}
