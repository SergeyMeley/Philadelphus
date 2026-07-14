using AutoMapper;
using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.Services;

public class PhiladelphusRepositoryServiceDeletionTests
{
    [Theory]
    [InlineData(State.ForSoftDelete)]
    [InlineData(State.ForHardDelete)]
    public void SetState_NewEntity_AllowsDeletionStates(State deletionState)
    {
        var workingTree = new FakeWorkingTreeModel();
        var writableTree = (IMainEntityWritableModel)workingTree;

        var result = writableTree.SetState(deletionState);

        result.Should().BeTrue();
        workingTree.State.Should().Be(deletionState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SoftDeleteShrubMember_WorkingTreeOrItsRoot_DeletesWholeWorkingTree(
        bool deleteViaRoot)
    {
        var notificationService = new FakeNotificationService();
        var workingTree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        workingTree.OwningShrub.AddContent(workingTree).Should().BeTrue();
        ((IMainEntityWritableModel)workingTree).SetState(State.SavedOrLoaded);
        ((IMainEntityWritableModel)root).SetState(State.SavedOrLoaded);
        ((IMainEntityWritableModel)workingTree.OwningShrub.OwningRepository).SetState(State.SavedOrLoaded);
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        IContentModel deletionEntryPoint = deleteViaRoot ? root : workingTree;

        var result = service.SoftDeleteShrubMember(deletionEntryPoint);

        result.Should().BeTrue();
        workingTree.State.Should().Be(State.ForSoftDelete);
        root.State.Should().Be(State.ForSoftDelete);
        workingTree.OwningShrub.ContentWorkingTreesUuids.Should().NotContain(workingTree.Uuid);
        workingTree.OwningShrub.OwningRepository.State.Should().Be(State.Changed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SoftDeleteShrubMember_NewWorkingTreeOrItsRoot_MarksWholeTreeForSoftDelete(
        bool deleteViaRoot)
    {
        var notificationService = new FakeNotificationService();
        var workingTree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        workingTree.OwningShrub.AddContent(workingTree).Should().BeTrue();
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        IContentModel deletionEntryPoint = deleteViaRoot ? root : workingTree;

        var result = service.SoftDeleteShrubMember(deletionEntryPoint);

        result.Should().BeTrue();
        workingTree.State.Should().Be(State.ForSoftDelete);
        root.State.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void SoftDeleteShrubMember_NewNode_MarksItForSoftDelete()
    {
        var notificationService = new FakeNotificationService();
        var workingTree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var result = service.SoftDeleteShrubMember(node);

        result.Should().BeTrue();
        node.State.Should().Be(State.ForSoftDelete);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HardDeleteShrubMember_WorkingTreeOrItsRoot_DeletesTreeDuringSave(
        bool deleteViaRoot)
    {
        var infrastructure = new Mock<IShrubMembersInfrastructureRepository>();
        infrastructure
            .Setup(x => x.HardDeleteTrees(It.IsAny<IEnumerable<WorkingTree>>()))
            .Returns(1);
        var dataStorage = new Mock<IDataStorageModel>();
        dataStorage.SetupGet(x => x.Uuid).Returns(Guid.CreateVersion7());
        dataStorage.SetupGet(x => x.Name).Returns("Тестовое хранилище");
        dataStorage.SetupGet(x => x.HasShrubMembersInfrastructureRepository).Returns(true);
        dataStorage.SetupGet(x => x.ShrubMembersInfrastructureRepository).Returns(infrastructure.Object);
        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            dataStorage.Object,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        var workingTree = new WorkingTreeModel(
            Guid.CreateVersion7(),
            dataStorage.Object,
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        var root = new TreeRootModel(
            Guid.CreateVersion7(),
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        repository.ContentShrub.AddContent(workingTree).Should().BeTrue();
        ((IMainEntityWritableModel)workingTree).SetState(State.SavedOrLoaded);
        ((IMainEntityWritableModel)root).SetState(State.SavedOrLoaded);
        ((IMainEntityWritableModel)repository).SetState(State.SavedOrLoaded);
        var mapper = new Mock<IMapper>();
        mapper
            .Setup(x => x.Map<List<WorkingTree>>(It.IsAny<object>()))
            .Returns(new List<WorkingTree> { new WorkingTree() });
        var service = new PhiladelphusRepositoryService(
            mapper.Object,
            Mock.Of<ILogger>(),
            notificationService);
        IContentModel deletionEntryPoint = deleteViaRoot ? root : workingTree;

        var result = service.HardDeleteShrubMember(deletionEntryPoint);

        result.Should().BeTrue();
        workingTree.State.Should().Be(State.ForHardDelete);
        root.State.Should().Be(State.ForHardDelete);
        repository.ContentShrub.ContentWorkingTrees.Should().Contain(workingTree);
        repository.ContentShrub.ContentWorkingTreesUuids.Should().NotContain(workingTree.Uuid);
        infrastructure.Verify(
            x => x.HardDeleteTrees(It.IsAny<IEnumerable<WorkingTree>>()),
            Times.Never);

        service.SaveChanges(new[] { workingTree }, SaveMode.OnlyHeader);

        repository.ContentShrub.ContentWorkingTrees.Should().NotContain(workingTree);
        infrastructure.Verify(
            x => x.HardDeleteTrees(It.IsAny<IEnumerable<WorkingTree>>()),
            Times.Once);
    }

    [Fact]
    public void SaveChanges_DeferredRootSequence_DoesNotReevaluateRemovedRoot()
    {
        var infrastructure = new Mock<IShrubMembersInfrastructureRepository>();
        infrastructure
            .Setup(x => x.SoftDeleteRoots(It.IsAny<IEnumerable<TreeRoot>>()))
            .Returns(1);
        var dataStorage = new Mock<IDataStorageModel>();
        dataStorage.SetupGet(x => x.Uuid).Returns(Guid.CreateVersion7());
        dataStorage.SetupGet(x => x.Name).Returns("Тестовое хранилище");
        dataStorage.SetupGet(x => x.HasShrubMembersInfrastructureRepository).Returns(true);
        dataStorage.SetupGet(x => x.ShrubMembersInfrastructureRepository).Returns(infrastructure.Object);
        var notificationService = new FakeNotificationService();
        var repository = new PhiladelphusRepositoryModel(
            Guid.CreateVersion7(),
            dataStorage.Object,
            notificationService,
            new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
            new EmptyPropertiesPolicy<ShrubModel>());
        var workingTree = new WorkingTreeModel(
            Guid.CreateVersion7(),
            dataStorage.Object,
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        var root = new TreeRootModel(
            Guid.NewGuid(),
            workingTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        workingTree.ContentRoot = root;
        var writableRoot = (IMainEntityWritableModel)root;
        writableRoot.SetState(State.SavedOrLoaded).Should().BeTrue();
        writableRoot.SetState(State.ForSoftDelete).Should().BeTrue();
        var mapper = new Mock<IMapper>();
        mapper
            .Setup(x => x.Map<List<TreeRoot>>(It.IsAny<object>()))
            .Returns(new List<TreeRoot> { new TreeRoot() });
        var service = new PhiladelphusRepositoryService(
            mapper.Object,
            Mock.Of<ILogger>(),
            notificationService);
        var deferredRoots = new[] { workingTree }
            .Select(x => x.ContentRoot!);

        var act = () => service.SaveChanges(deferredRoots, SaveMode.WithContentAndMembers);

        act.Should().NotThrow();
        workingTree.ContentRoot.Should().BeNull();
        infrastructure.Verify(
            x => x.SoftDeleteRoots(It.IsAny<IEnumerable<TreeRoot>>()),
            Times.Once);
    }
}
