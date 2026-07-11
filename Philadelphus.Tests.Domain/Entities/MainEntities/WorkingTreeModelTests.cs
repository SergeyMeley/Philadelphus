using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Entities.MainEntities;

public class WorkingTreeModelTests
{
    [Fact]
    public void ChangeDataStorage_AvailableSupportedStorage_ChangesStorage()
    {
        var originalStorage = CreateDataStorage(hasShrubMembersRepository: true);
        var newStorage = CreateDataStorage(hasShrubMembersRepository: true);
        var repository = CreateRepository(originalStorage);
        repository.AddAvailableDataStorage(newStorage);
        var sut = CreateWorkingTree(repository, originalStorage);

        var result = sut.ChangeDataStorage(newStorage);

        result.Should().BeTrue();
        sut.OwnDataStorage.Should().BeSameAs(newStorage);
    }

    [Fact]
    public void ChangeDataStorage_CurrentStorage_ReturnsFalse()
    {
        var storage = CreateDataStorage(hasShrubMembersRepository: true);
        var repository = CreateRepository(storage);
        var sut = CreateWorkingTree(repository, storage);

        var result = sut.ChangeDataStorage(storage);

        result.Should().BeFalse();
        sut.OwnDataStorage.Should().BeSameAs(storage);
    }

    [Fact]
    public void ChangeDataStorage_UnavailableStorage_ThrowsInvalidOperationException()
    {
        var originalStorage = CreateDataStorage(hasShrubMembersRepository: true);
        var unavailableStorage = CreateDataStorage(hasShrubMembersRepository: true);
        var repository = CreateRepository(originalStorage);
        var sut = CreateWorkingTree(repository, originalStorage);

        sut.Invoking(x => x.ChangeDataStorage(unavailableStorage))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*не входит в список возможных хранилищ*");
    }

    [Fact]
    public void ChangeDataStorage_UnsupportedStorage_ThrowsInvalidOperationException()
    {
        var originalStorage = CreateDataStorage(hasShrubMembersRepository: true);
        var unsupportedStorage = CreateDataStorage(hasShrubMembersRepository: false);
        var repository = CreateRepository(originalStorage);
        repository.AddAvailableDataStorage(unsupportedStorage);
        var sut = CreateWorkingTree(repository, originalStorage);

        sut.Invoking(x => x.ChangeDataStorage(unsupportedStorage))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*не поддерживает участников кустарника*");
    }

    [Fact]
    public void ChangeDataStorage_NullStorage_ThrowsArgumentNullException()
    {
        var storage = CreateDataStorage(hasShrubMembersRepository: true);
        var repository = CreateRepository(storage);
        var sut = CreateWorkingTree(repository, storage);

        sut.Invoking(x => x.ChangeDataStorage(null!))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("storage");
    }

    private static WorkingTreeModel CreateWorkingTree(
        PhiladelphusRepositoryModel repository,
        IDataStorageModel storage)
    {
        return new WorkingTreeModel(
            Guid.CreateVersion7(),
            storage,
            repository.ContentShrub,
            new FakeNotificationService(),
            new EmptyPropertiesPolicy<WorkingTreeModel>());
    }

    private static PhiladelphusRepositoryModel CreateRepository(IDataStorageModel storage)
    {
        return new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            storage,
            new FakeNotificationService(),
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
    }

    private static IDataStorageModel CreateDataStorage(bool hasShrubMembersRepository)
    {
        var storage = new Mock<IDataStorageModel>();
        storage.SetupGet(x => x.Uuid).Returns(Guid.CreateVersion7());
        storage.SetupGet(x => x.Name).Returns("Тестовое хранилище");
        storage.SetupGet(x => x.HasShrubMembersInfrastructureRepository)
            .Returns(hasShrubMembersRepository);
        return storage.Object;
    }
}
