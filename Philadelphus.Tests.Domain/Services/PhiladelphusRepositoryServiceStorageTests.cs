using AutoMapper;
using FluentAssertions;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.Services;

public class PhiladelphusRepositoryServiceStorageTests
{
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
}
