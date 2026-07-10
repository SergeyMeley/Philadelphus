using FluentAssertions;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Tests.Domain.Configurations;

public class DataStoragesConnectionStringsMigratorTests
{
    [Fact]
    public void Migrate_EmptyDataStorageConnectionStrings_CopiesLegacyConnectionStrings()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var dataStorage = CreateDataStorage(storageUuid);
        var dataStoragesConfig = new DataStoragesCollectionConfig
        {
            DataStorages = new List<DataStorage> { dataStorage }
        };
        var connectionStringsConfig = new ConnectionStringsCollectionConfig
        {
            ConnectionStringsContainers = new List<ConnectionStringsContainer>
            {
                CreateConnectionStringsContainer(
                    storageUuid,
                    providerName: "Legacy provider",
                    connectionString: "legacy-connection")
            }
        };

        // Act
        var result = DataStoragesConnectionStringsMigrator.Migrate(
            dataStoragesConfig,
            connectionStringsConfig);

        // Assert
        result.HasChanges.Should().BeTrue();
        result.MigratedDataStorages.Should().ContainSingle().Which.Should().BeSameAs(dataStorage);
        result.DataStoragesWithoutLegacyConnectionStrings.Should().BeEmpty();
        dataStorage.ProviderName.Should().Be("Legacy provider");
        dataStorage.ConnectionStrings[InfrastructureEntityGroups.PhiladelphusRepositories]
            .Should().Be("legacy-connection");
    }

    [Fact]
    public void Migrate_AlreadyFilledDataStorageConnectionStrings_DoesNotOverwriteConnectionStrings()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var dataStorage = CreateDataStorage(storageUuid);
        dataStorage.ProviderName = "Embedded provider";
        dataStorage.ConnectionStrings = new Dictionary<InfrastructureEntityGroups, string>
        {
            [InfrastructureEntityGroups.PhiladelphusRepositories] = "embedded-connection"
        };
        var dataStoragesConfig = new DataStoragesCollectionConfig
        {
            DataStorages = new List<DataStorage> { dataStorage }
        };
        var connectionStringsConfig = new ConnectionStringsCollectionConfig
        {
            ConnectionStringsContainers = new List<ConnectionStringsContainer>
            {
                CreateConnectionStringsContainer(
                    storageUuid,
                    providerName: "Legacy provider",
                    connectionString: "legacy-connection")
            }
        };

        // Act
        var result = DataStoragesConnectionStringsMigrator.Migrate(
            dataStoragesConfig,
            connectionStringsConfig);

        // Assert
        result.HasChanges.Should().BeFalse();
        result.MigratedDataStorages.Should().BeEmpty();
        dataStorage.ProviderName.Should().Be("Embedded provider");
        dataStorage.ConnectionStrings[InfrastructureEntityGroups.PhiladelphusRepositories]
            .Should().Be("embedded-connection");
    }

    [Fact]
    public void Migrate_MissingLegacyConnectionStrings_CreatesEmptyConnectionStringsAndReturnsStorage()
    {
        // Arrange
        var dataStorage = CreateDataStorage(Guid.CreateVersion7());
        var dataStoragesConfig = new DataStoragesCollectionConfig
        {
            DataStorages = new List<DataStorage> { dataStorage }
        };
        var connectionStringsConfig = new ConnectionStringsCollectionConfig
        {
            ConnectionStringsContainers = new List<ConnectionStringsContainer>()
        };

        // Act
        var result = DataStoragesConnectionStringsMigrator.Migrate(
            dataStoragesConfig,
            connectionStringsConfig);

        // Assert
        result.HasChanges.Should().BeTrue();
        result.MigratedDataStorages.Should().BeEmpty();
        result.DataStoragesWithoutLegacyConnectionStrings.Should().ContainSingle().Which.Should().BeSameAs(dataStorage);
        dataStorage.ConnectionStrings.Should().BeEmpty();
    }

    private static DataStorage CreateDataStorage(Guid uuid)
    {
        return new DataStorage
        {
            Uuid = uuid,
            Name = "Test storage",
            Description = "Test storage",
            InfrastructureType = InfrastructureTypes.SQLiteEf
        };
    }

    private static ConnectionStringsContainer CreateConnectionStringsContainer(
        Guid storageUuid,
        string providerName,
        string connectionString)
    {
        return new ConnectionStringsContainer
        {
            StorageUuid = storageUuid,
            InfrastructureType = InfrastructureTypes.SQLiteEf,
            ProviderName = providerName,
            ConnectionStrings = new Dictionary<InfrastructureEntityGroups, string>
            {
                [InfrastructureEntityGroups.PhiladelphusRepositories] = connectionString
            }
        };
    }
}
