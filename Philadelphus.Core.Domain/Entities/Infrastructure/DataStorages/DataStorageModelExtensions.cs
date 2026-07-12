using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Методы определения возможностей хранилища данных.
    /// </summary>
    public static class DataStorageModelExtensions
    {
        /// <summary>
        /// Определяет, поддерживает ли хранилище указанную группу сущностей.
        /// Учитывает как созданный инфраструктурный репозиторий, так и настроенную строку подключения.
        /// </summary>
        /// <param name="dataStorage">Хранилище данных.</param>
        /// <param name="entityGroup">Группа сущностей.</param>
        /// <returns>true, если группа поддерживается; иначе false.</returns>
        public static bool SupportsEntityGroup(
            this IDataStorageModel dataStorage,
            InfrastructureEntityGroups entityGroup)
        {
            ArgumentNullException.ThrowIfNull(dataStorage);

            var hasInfrastructureRepository = entityGroup switch
            {
                InfrastructureEntityGroups.PhiladelphusRepositories =>
                    dataStorage.HasPhiladelphusRepositoriesInfrastructureRepository,
                InfrastructureEntityGroups.ShrubMembers =>
                    dataStorage.HasShrubMembersInfrastructureRepository,
                InfrastructureEntityGroups.Reports =>
                    dataStorage.HasReportsInfrastructureRepository,
                _ => false
            };
            if (hasInfrastructureRepository)
                return true;

            return dataStorage.ConnectionStrings != null
                && dataStorage.ConnectionStrings.TryGetValue(entityGroup, out var connectionString)
                && string.IsNullOrWhiteSpace(connectionString) == false;
        }
    }
}
