using Npgsql;
using Philadelphus.Presentation.Infrastructure;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления редактируемых параметров строки подключения PostgreSQL.
    /// </summary>
    public class PostgreSqlConnectionStringVM : ViewModelBase
    {
        private string _host = string.Empty;
        private int _port = 5432;
        private string _database = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public ObservableCollection<ConnectionStringParameterVM> AdditionalParameters { get; } = new();

        /// <summary>Сервер PostgreSQL.</summary>
        public string Host { get => _host; set => SetField(ref _host, value, nameof(Host)); }
        /// <summary>Порт PostgreSQL.</summary>
        public int Port { get => _port; set => SetField(ref _port, value, nameof(Port)); }
        /// <summary>Наименование базы данных.</summary>
        public string Database { get => _database; set => SetField(ref _database, value, nameof(Database)); }
        /// <summary>Имя пользователя.</summary>
        public string Username { get => _username; set => SetField(ref _username, value, nameof(Username)); }
        /// <summary>Пароль пользователя.</summary>
        public string Password { get => _password; set => SetField(ref _password, value, nameof(Password)); }

        /// <summary>Строка подключения, построенная штатным Npgsql builder.</summary>
        public string ConnectionString
        {
            get => TryBuildConnectionString(out var result) ? result : string.Empty;
            set
            {
                NpgsqlConnectionStringBuilder builder;
                try
                {
                    builder = new NpgsqlConnectionStringBuilder(value ?? string.Empty);
                }
                catch (ArgumentException)
                {
                    _host = string.Empty;
                    _database = string.Empty;
                    _username = string.Empty;
                    _password = string.Empty;
                    AdditionalParameters.Clear();
                    NotifyConnectionStringChanged();
                    return;
                }
                _host = builder.Host;
                _port = builder.Port;
                _database = builder.Database ?? string.Empty;
                _username = builder.Username ?? string.Empty;
                _password = builder.Password ?? string.Empty;
                AdditionalParameters.Clear();
                foreach (KeyValuePair<string, object> parameter in builder)
                {
                    if (FixedKeys.Contains(parameter.Key))
                        continue;

                    AdditionalParameters.Add(new ConnectionStringParameterVM(
                        parameter.Key,
                        parameter.Value?.ToString() ?? string.Empty));
                }
                NotifyConnectionStringChanged();
            }
        }

        /// <summary>Признак корректно заполненных обязательных и дополнительных параметров.</summary>
        public bool IsValid => string.IsNullOrWhiteSpace(Host) == false
            && Port is > 0 and <= 65535
            && string.IsNullOrWhiteSpace(Database) == false
            && string.IsNullOrWhiteSpace(Username) == false
            && AdditionalParameters.All(x => string.IsNullOrWhiteSpace(x.Key) == false)
            && AdditionalParameters.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).All(x => x.Count() == 1)
            && TryBuildConnectionString(out _);

        /// <summary>Признак отсутствия пользовательских параметров подключения.</summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Host)
            && string.IsNullOrWhiteSpace(Database)
            && string.IsNullOrWhiteSpace(Username)
            && string.IsNullOrWhiteSpace(Password)
            && AdditionalParameters.Count == 0;

        public PostgreSqlConnectionStringVM(string connectionString = "")
        {
            AdditionalParameters.CollectionChanged += AdditionalParametersCollectionChanged;
            ConnectionString = connectionString;
        }

        public RelayCommand AddAdditionalParameterCommand
        {
            get { return new RelayCommand(_ => AdditionalParameters.Add(new(string.Empty, string.Empty))); }
        }

        public RelayCommand RemoveAdditionalParameterCommand
        {
            get
            {
                return new RelayCommand(
                    parameter => AdditionalParameters.Remove((ConnectionStringParameterVM)parameter),
                    parameter => parameter is ConnectionStringParameterVM vm && AdditionalParameters.Contains(vm));
            }
        }

        public RelayCommand OpenDocumentationCommand
        {
            get
            {
                return new RelayCommand(_ => Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.npgsql.org/doc/connection-string-parameters.html",
                    UseShellExecute = true
                }));
            }
        }

        private static readonly HashSet<string> FixedKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Host", "Port", "Database", "Username", "Password"
        };

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;
            field = value;
            OnPropertyChanged(propertyName);
            NotifyConnectionStringChanged();
        }

        private void AdditionalParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ConnectionStringParameterVM item in e.OldItems)
                    item.PropertyChanged -= AdditionalParameterPropertyChanged;
            if (e.NewItems != null)
                foreach (ConnectionStringParameterVM item in e.NewItems)
                    item.PropertyChanged += AdditionalParameterPropertyChanged;
            NotifyConnectionStringChanged();
        }

        private void AdditionalParameterPropertyChanged(object? sender, PropertyChangedEventArgs e) => NotifyConnectionStringChanged();

        private void NotifyConnectionStringChanged()
        {
            OnPropertyChanged(nameof(ConnectionString));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private bool TryBuildConnectionString(out string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = Host,
                    Port = Port,
                    Database = Database,
                    Username = Username,
                    Password = Password
                };
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
