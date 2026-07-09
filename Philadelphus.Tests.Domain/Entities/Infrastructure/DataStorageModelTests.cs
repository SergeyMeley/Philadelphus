using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Tests.Domain.Entities.Infrastructure;

public class DataStorageModelTests
{
    [Fact]
    public void MainDataStorage_WhenSetHiddenOrDisabled_StaysVisibleAndEnabled()
    {
        // Arrange
        var sut = CreateDataStorage(DataStorageModel.MainDataStorageUuid);

        // Act
        sut.IsHidden = true;
        sut.IsDisabled = true;

        // Assert
        sut.IsMainDataStorage.Should().BeTrue();
        sut.IsHidden.Should().BeFalse();
        sut.IsDisabled.Should().BeFalse();
    }

    [Fact]
    public void MainDataStorage_WhenSetSettings_StaysWithInitialSettings()
    {
        // Arrange
        var sut = CreateDataStorage(DataStorageModel.MainDataStorageUuid);

        // Act
        sut.Name = "New storage";
        sut.Description = "New description";
        sut.ProviderName = "New provider";
        sut.InfrastructureType = InfrastructureTypes.PostgreSqlEf;

        // Assert
        sut.Name.Should().Be("Test storage");
        sut.Description.Should().Be("Test storage");
        sut.ProviderName.Should().BeEmpty();
        sut.InfrastructureType.Should().Be(InfrastructureTypes.SQLiteEf);
    }

    [Fact]
    public void RegularDataStorage_WhenSetHiddenOrDisabled_ChangesState()
    {
        // Arrange
        var sut = CreateDataStorage(Guid.CreateVersion7());

        // Act
        sut.IsHidden = true;
        sut.IsDisabled = true;

        // Assert
        sut.IsMainDataStorage.Should().BeFalse();
        sut.IsHidden.Should().BeTrue();
        sut.IsDisabled.Should().BeTrue();
    }

    [Fact]
    public void RegularDataStorage_WhenSetSettings_ChangesSettings()
    {
        // Arrange
        var sut = CreateDataStorage(Guid.CreateVersion7());

        // Act
        sut.Name = "New storage";
        sut.Description = "New description";
        sut.ProviderName = "New provider";
        sut.InfrastructureType = InfrastructureTypes.PostgreSqlEf;

        // Assert
        sut.Name.Should().Be("New storage");
        sut.Description.Should().Be("New description");
        sut.ProviderName.Should().Be("New provider");
        sut.InfrastructureType.Should().Be(InfrastructureTypes.PostgreSqlEf);
    }

    private static DataStorageModel CreateDataStorage(Guid uuid)
    {
        var repository = new Mock<IPhiladelphusRepositoriesInfrastructureRepository>();
        repository.Setup(x => x.EntityGroup).Returns(InfrastructureEntityGroups.PhiladelphusRepositories);
        repository.Setup(x => x.CheckAvailability()).Returns(true);

        return (DataStorageModel)new DataStorageBuilder()
            .SetGeneralParameters(
                logger: Mock.Of<ILogger>(),
                name: "Test storage",
                description: "Test storage",
                uuid: uuid,
                infrastructureType: InfrastructureTypes.SQLiteEf,
                isDisabled: false)
            .SetRepository(repository.Object)
            .Build();
    }
}
