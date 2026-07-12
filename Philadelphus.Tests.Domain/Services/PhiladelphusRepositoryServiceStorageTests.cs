using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.Services;

public class PhiladelphusRepositoryServiceStorageTests
{
    [Fact]
    public void SaveChanges_SystemWorkingTree_UsesMainDataStorage()
    {
        // Arrange
        var mainInfrastructure = new Mock<IShrubMembersInfrastructureRepository>();
        mainInfrastructure.Setup(x => x.InsertTrees(It.IsAny<IEnumerable<WorkingTree>>()))
            .Returns(1);
        var secondaryInfrastructure = new Mock<IShrubMembersInfrastructureRepository>();

        var mainStorage = CreateShrubMembersStorage(
            DataStorageModel.MainDataStorageUuid,
            "Основное хранилище",
            mainInfrastructure.Object);
        var secondaryStorage = CreateShrubMembersStorage(
            Guid.CreateVersion7(),
            "Дополнительное хранилище",
            secondaryInfrastructure.Object);
        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            secondaryStorage,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        repository.AddAvailableDataStorage(mainStorage);
        var systemTree = new WorkingTreeModel(
            WorkingTreeModel.SystemBaseUuid,
            secondaryStorage,
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<List<WorkingTree>>(It.IsAny<object>()))
            .Returns(new List<WorkingTree> { new WorkingTree() });
        var sut = new PhiladelphusRepositoryService(
            mapper.Object,
            Mock.Of<ILogger>(),
            notificationService);

        // Act
        sut.SaveChanges(new[] { systemTree }, SaveMode.OnlyHeader);

        // Assert
        systemTree.DataStorage.Should().BeSameAs(mainStorage);
        mainInfrastructure.Verify(
            x => x.InsertTrees(It.IsAny<IEnumerable<WorkingTree>>()),
            Times.Once);
        secondaryInfrastructure.Verify(
            x => x.InsertTrees(It.IsAny<IEnumerable<WorkingTree>>()),
            Times.Never);
        var changeStorage = () => systemTree.ChangeDataStorage(secondaryStorage);
        changeStorage.Should().Throw<InvalidOperationException>()
            .WithMessage("*только в основном хранилище*");
    }

    [Fact]
    public void TreeLeave_DataStorage_UsesLoadedSourceOtherwiseWorkingTreeStorage()
    {
        // Arrange
        var treeStorage = CreateShrubMembersStorage(
            Guid.CreateVersion7(),
            "Хранилище дерева",
            Mock.Of<IShrubMembersInfrastructureRepository>());
        var loadedSourceStorage = CreateShrubMembersStorage(
            Guid.CreateVersion7(),
            "Исходное хранилище листа",
            Mock.Of<IShrubMembersInfrastructureRepository>());
        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            treeStorage,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        var tree = new WorkingTreeModel(
            Guid.CreateVersion7(),
            treeStorage,
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        var root = new TreeRootModel(
            Guid.CreateVersion7(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        tree.ContentRoot = root;
        var node = new TreeNodeModel(
            Guid.CreateVersion7(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());

        // Act
        var newLeave = new TreeLeaveModel(
            Guid.CreateVersion7(),
            node,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var loadedLeave = new TreeLeaveModel(
            Guid.CreateVersion7(),
            node,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>(),
            loadedSourceStorage);

        // Assert
        newLeave.DataStorage.Should().BeSameAs(treeStorage);
        loadedLeave.DataStorage.Should().BeSameAs(loadedSourceStorage);
    }

    [Fact]
    public void SaveChanges_DatabaseFailure_PreservesStateAndSendsClearError()
    {
        // Arrange
        var databaseException = new DbUpdateException("Ошибка тестовой БД");
        var infrastructure = new Mock<IPhiladelphusRepositoriesInfrastructureRepository>();
        infrastructure.Setup(x => x.InsertRepository(It.IsAny<PhiladelphusRepository>()))
            .Throws(databaseException);

        var storage = new Mock<IDataStorageModel>();
        storage.SetupGet(x => x.Uuid).Returns(Guid.CreateVersion7());
        storage.SetupGet(x => x.Name).Returns("Тестовое хранилище");
        storage.SetupGet(x => x.PhiladelphusRepositoriesInfrastructureRepository)
            .Returns(infrastructure.Object);

        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            storage.Object,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        var originalState = repository.State;

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<PhiladelphusRepository>(repository))
            .Returns(new PhiladelphusRepository());
        var sut = new PhiladelphusRepositoryService(
            mapper.Object,
            Mock.Of<ILogger>(),
            notificationService);

        // Act
        var action = () => sut.SaveChanges(ref repository, SaveMode.OnlyHeader);

        // Assert
        action.Should().Throw<DbUpdateException>();
        repository.State.Should().Be(originalState);
        notificationService.Messages.Should().Contain(
            $"Ошибка сохранения изменений в БД. Подробнее:\r\n{databaseException.Message}");
    }

    [Fact]
    public void SaveChanges_UnsupportedShrubMembersStorage_ThrowsClearError()
    {
        // Arrange
        var storageUuid = Guid.CreateVersion7();
        var storage = new Mock<IDataStorageModel>();
        storage.SetupGet(x => x.Uuid).Returns(storageUuid);
        storage.SetupGet(x => x.Name).Returns("Хранилище отчётов");
        storage.SetupGet(x => x.HasShrubMembersInfrastructureRepository).Returns(false);
        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            storage.Object,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        var tree = new WorkingTreeModel(
            Guid.CreateVersion7(),
            storage.Object,
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        var sut = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        // Act
        var action = () => sut.SaveChanges(new[] { tree }, SaveMode.OnlyHeader);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Хранилище отчётов*{storageUuid}*не поддерживает сохранение элементов кустарника*");
    }

    private static IDataStorageModel CreateShrubMembersStorage(
        Guid uuid,
        string name,
        IShrubMembersInfrastructureRepository infrastructure)
    {
        var storage = new Mock<IDataStorageModel>();
        storage.SetupGet(x => x.Uuid).Returns(uuid);
        storage.SetupGet(x => x.Name).Returns(name);
        storage.SetupGet(x => x.HasShrubMembersInfrastructureRepository).Returns(true);
        storage.SetupGet(x => x.ShrubMembersInfrastructureRepository).Returns(infrastructure);
        return storage.Object;
    }
}
