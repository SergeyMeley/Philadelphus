using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using System.ComponentModel;
using System.IO;

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

        public bool CanFillFromRepositories
        {
            get { return EntityGroup != InfrastructureEntityGroups.PhiladelphusRepositories; }
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

        /// <summary>
        /// Заполняет параметры из строки репозиториев с учетом типа инфраструктуры.
        /// </summary>
        public void FillFromRepositories(
            DataStorageConnectionStringVM source,
            InfrastructureTypes infrastructureType)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (CanFillFromRepositories == false)
                return;

            if (infrastructureType == InfrastructureTypes.SQLiteEf)
            {
                CopySqliteSettings(source.SqliteEditor!, SqliteEditor!);
                return;
            }
            if (infrastructureType == InfrastructureTypes.PostgreSqlEf)
            {
                CopyPostgreSqlSettings(source.PostgreSqlEditor!, PostgreSqlEditor!);
                return;
            }

            ConnectionString = source.ConnectionString;
        }

        private void CopySqliteSettings(SqliteConnectionStringVM source, SqliteConnectionStringVM target)
        {
            target.DataSource = BuildDistinctSqlitePath(source.DataSource);
            target.AdditionalParameters.Clear();
            foreach (var parameter in source.AdditionalParameters)
                target.AdditionalParameters.Add(new(parameter.Key, parameter.Value));
        }

        private static void CopyPostgreSqlSettings(
            PostgreSqlConnectionStringVM source,
            PostgreSqlConnectionStringVM target)
        {
            target.Host = source.Host;
            target.Port = source.Port;
            target.Database = source.Database;
            target.Username = source.Username;
            target.Password = source.Password;
            target.AdditionalParameters.Clear();
            foreach (var parameter in source.AdditionalParameters)
                target.AdditionalParameters.Add(new(parameter.Key, parameter.Value));
        }

        private string BuildDistinctSqlitePath(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return string.Empty;

            var suffix = EntityGroup == InfrastructureEntityGroups.ShrubMembers
                ? "-shrub-members"
                : "-reports";
            if (sourcePath == ":memory:")
                return $"file:philadelphus{suffix}?mode=memory&cache=shared";

            var directory = Path.GetDirectoryName(sourcePath);
            var fileName = Path.GetFileNameWithoutExtension(sourcePath) + suffix + Path.GetExtension(sourcePath);
            return string.IsNullOrEmpty(directory) ? fileName : Path.Combine(directory, fileName);
        }
    }
}
