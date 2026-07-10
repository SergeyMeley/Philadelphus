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
