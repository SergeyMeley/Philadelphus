using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Переносит legacy-строки подключения в конфигурацию хранилищ данных.
    /// </summary>
    public static class DataStoragesConnectionStringsMigrator
    {
        /// <summary>
        /// Перенести legacy-строки подключения в конфигурацию хранилищ данных.
        /// </summary>
        /// <param name="dataStoragesConfig">Конфигурация хранилищ данных.</param>
        /// <param name="connectionStringsConfig">Legacy-конфигурация строк подключения.</param>
        /// <returns>Результат переноса.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public static DataStoragesConnectionStringsMigrationResult Migrate(
            DataStoragesCollectionConfig dataStoragesConfig,
            ConnectionStringsCollectionConfig connectionStringsConfig)
        {
            ArgumentNullException.ThrowIfNull(dataStoragesConfig);
            ArgumentNullException.ThrowIfNull(connectionStringsConfig);
            ArgumentNullException.ThrowIfNull(dataStoragesConfig.DataStorages);
            ArgumentNullException.ThrowIfNull(connectionStringsConfig.ConnectionStringsContainers);

            var migratedDataStorages = new List<DataStorage>();
            var dataStoragesWithoutLegacyConnectionStrings = new List<DataStorage>();

            foreach (var dataStorage in dataStoragesConfig.DataStorages)
            {
                if (dataStorage.ConnectionStrings != null && dataStorage.ConnectionStrings.Count > 0)
                    continue;

                var legacyConnectionStrings = connectionStringsConfig.ConnectionStringsContainers
                    .SingleOrDefault(x => x.StorageUuid == dataStorage.Uuid);
                if (legacyConnectionStrings == null)
                {
                    dataStorage.ConnectionStrings = new();
                    dataStoragesWithoutLegacyConnectionStrings.Add(dataStorage);
                    continue;
                }

                dataStorage.ProviderName = legacyConnectionStrings.ProviderName ?? string.Empty;
                dataStorage.ConnectionStrings = legacyConnectionStrings.ConnectionStrings == null
                    ? new()
                    : new(legacyConnectionStrings.ConnectionStrings);
                migratedDataStorages.Add(dataStorage);
            }

            return new DataStoragesConnectionStringsMigrationResult(
                migratedDataStorages,
                dataStoragesWithoutLegacyConnectionStrings);
        }
    }
}
