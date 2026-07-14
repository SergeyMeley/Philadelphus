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
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.Services;

public class PhiladelphusRepositoryServiceDeletionTests
{
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
        ((IMainEntityWritableModel)root).SetState(State.SavedOrLoaded);
        ((IMainEntityWritableModel)root).SetState(State.ForSoftDelete);
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
