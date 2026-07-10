using FluentAssertions;

using Moq;

using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Entities.MainEntities;

public class PhiladelphusRepositoryDefaultDataStoragesTests
{
    [Fact]
    public void Constructor_OwnDataStorageSupportsGroups_SetsDefaultDataStorages()
    {
        // Arrange
        var dataStorage = CreateDataStorage(
            hasShrubMembersRepository: true,
            hasReportsRepository: true);

        // Act
        var sut = CreateSut(dataStorage);

        // Assert
        sut.DefaultShrubMembersDataStorage.Should().BeSameAs(dataStorage);
        sut.DefaultReportsDataStorage.Should().BeSameAs(dataStorage);
    }

    [Fact]
    public void Constructor_OwnDataStorageDoesNotSupportGroups_LeavesDefaultDataStoragesEmpty()
    {
        // Arrange
        var dataStorage = CreateDataStorage(
            hasShrubMembersRepository: false,
            hasReportsRepository: false);

        // Act
        var sut = CreateSut(dataStorage);

        // Assert
        sut.DefaultShrubMembersDataStorage.Should().BeNull();
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    [Fact]
    public void AddAvailableDataStorage_SupportedStorage_DoesNotSetDefaults()
    {
        // Arrange
        var sut = CreateSut(CreateDataStorage(false, false));
        var availableStorage = CreateDataStorage(true, true);

        // Act
        var result = sut.AddAvailableDataStorage(availableStorage);

        // Assert
        result.Should().BeTrue();
        sut.DataStorages.Should().Contain(availableStorage);
        sut.DefaultShrubMembersDataStorage.Should().BeNull();
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    [Fact]
    public void SetDefaultShrubMembersDataStorage_AvailableSupportedStorage_SetsDefault()
    {
        // Arrange
        var sut = CreateSut(CreateDataStorage(false, false));
        var availableStorage = CreateDataStorage(true, false);
        sut.AddAvailableDataStorage(availableStorage);

        // Act
        var result = sut.SetDefaultShrubMembersDataStorage(availableStorage);

        // Assert
        result.Should().BeTrue();
        sut.DefaultShrubMembersDataStorage.Should().BeSameAs(availableStorage);
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    [Fact]
    public void SetDefaultReportsDataStorage_UnavailableStorage_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut(CreateDataStorage(false, false));
        var unavailableStorage = CreateDataStorage(false, true);

        // Act
        var act = () => sut.SetDefaultReportsDataStorage(unavailableStorage);

        // Assert
        act.Should().Throw<InvalidOperationException>();
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    [Fact]
    public void SetDefaultReportsDataStorage_UnsupportedStorage_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut(CreateDataStorage(false, false));
        var unsupportedStorage = CreateDataStorage(true, false);
        sut.AddAvailableDataStorage(unsupportedStorage);

        // Act
        var act = () => sut.SetDefaultReportsDataStorage(unsupportedStorage);

        // Assert
        act.Should().Throw<InvalidOperationException>();
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    [Fact]
    public void SetDefaultReportsDataStorage_Null_ClearsDefault()
    {
        // Arrange
        var dataStorage = CreateDataStorage(false, true);
        var sut = CreateSut(dataStorage);

        // Act
        var result = sut.SetDefaultReportsDataStorage(null);

        // Assert
        result.Should().BeTrue();
        sut.DefaultReportsDataStorage.Should().BeNull();
    }

    private static PhiladelphusRepositoryModel CreateSut(IDataStorageModel dataStorage)
    {
        return new PhiladelphusRepositoryDefaultDataStoragesTestingFixture(
            Guid.CreateVersion7(),
            dataStorage);
    }

    private static IDataStorageModel CreateDataStorage(
        bool hasShrubMembersRepository,
        bool hasReportsRepository)
    {
        var dataStorage = new Mock<IDataStorageModel>();
        dataStorage.Setup(x => x.Uuid).Returns(Guid.CreateVersion7());
        dataStorage.Setup(x => x.Name).Returns("Test storage");
        dataStorage.Setup(x => x.HasShrubMembersInfrastructureRepository)
            .Returns(hasShrubMembersRepository);
        dataStorage.Setup(x => x.HasReportsInfrastructureRepository)
            .Returns(hasReportsRepository);
        return dataStorage.Object;
    }
}

internal class PhiladelphusRepositoryDefaultDataStoragesTestingFixture : PhiladelphusRepositoryModel
{
    public PhiladelphusRepositoryDefaultDataStoragesTestingFixture(
        Guid uuid,
        IDataStorageModel dataStorage)
        : base(
            uuid,
            dataStorage,
            new FakeNotificationService(),
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>())
    {
    }
}
