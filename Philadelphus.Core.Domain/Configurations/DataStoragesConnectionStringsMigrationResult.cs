using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Результат переноса строк подключения в конфигурацию хранилищ данных.
    /// </summary>
    public class DataStoragesConnectionStringsMigrationResult
    {
        /// <summary>
        /// Хранилища данных, для которых строки подключения перенесены.
        /// </summary>
        public IReadOnlyCollection<DataStorage> MigratedDataStorages { get; }

        /// <summary>
        /// Хранилища данных, для которых не найден legacy-контейнер строк подключения.
        /// </summary>
        public IReadOnlyCollection<DataStorage> DataStoragesWithoutLegacyConnectionStrings { get; }

        /// <summary>
        /// Требуется сохранить новый конфиг хранилищ данных.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return MigratedDataStorages.Count > 0
                    || DataStoragesWithoutLegacyConnectionStrings.Count > 0;
            }
        }

        /// <summary>
        /// Результат переноса строк подключения в конфигурацию хранилищ данных.
        /// </summary>
        /// <param name="migratedDataStorages">Хранилища данных, для которых строки подключения перенесены.</param>
        /// <param name="dataStoragesWithoutLegacyConnectionStrings">Хранилища данных, для которых не найден legacy-контейнер строк подключения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public DataStoragesConnectionStringsMigrationResult(
            IReadOnlyCollection<DataStorage> migratedDataStorages,
            IReadOnlyCollection<DataStorage> dataStoragesWithoutLegacyConnectionStrings)
        {
            ArgumentNullException.ThrowIfNull(migratedDataStorages);
            ArgumentNullException.ThrowIfNull(dataStoragesWithoutLegacyConnectionStrings);

            MigratedDataStorages = migratedDataStorages;
            DataStoragesWithoutLegacyConnectionStrings = dataStoragesWithoutLegacyConnectionStrings;
        }
    }
}
