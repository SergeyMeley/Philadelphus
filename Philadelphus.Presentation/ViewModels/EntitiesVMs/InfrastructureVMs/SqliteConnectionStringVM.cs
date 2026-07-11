using Microsoft.Data.Sqlite;
using Philadelphus.Presentation.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

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
                return TryBuildConnectionString(out var connectionString)
                    ? connectionString
                    : string.Empty;
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

        public bool IsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(DataSource) == false
                    && AdditionalParameters.All(x => string.IsNullOrWhiteSpace(x.Key) == false)
                    && AdditionalParameters.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).All(x => x.Count() == 1)
                    && TryBuildConnectionString(out _);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(DataSource)
                    && AdditionalParameters.Count == 0;
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

        /// <summary>
        /// Открывает официальную документацию по строкам подключения SQLite.
        /// </summary>
        public RelayCommand OpenDocumentationCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://learn.microsoft.com/dotnet/standard/data/sqlite/connection-strings",
                        UseShellExecute = true
                    });
                });
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
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void AdditionalParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectionString));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private bool TryBuildConnectionString(out string connectionString)
        {
            try
            {
                var builder = new SqliteConnectionStringBuilder { DataSource = DataSource };
                foreach (var parameter in AdditionalParameters.Where(x => string.IsNullOrWhiteSpace(x.Key) == false))
                    builder[parameter.Key] = parameter.Value;

                connectionString = builder.ConnectionString;
                return true;
            }
            catch (ArgumentException)
            {
                connectionString = string.Empty;
                return false;
            }
        }
    }
}
