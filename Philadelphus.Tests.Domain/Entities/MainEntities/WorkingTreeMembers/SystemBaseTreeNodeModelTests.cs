using FluentAssertions;
using AutoMapper;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;
using System.Reflection;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.WorkingTreeMembers;

public class SystemBaseTreeNodeModelTests
{
    [Theory]
    [MemberData(nameof(SystemBaseTypes))]
    public void Constructor_SystemBaseType_CreatesNodeWithInitializedProperties(SystemBaseType type)
    {
        // Arrange
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());

        // Act
        var sut = new SystemBaseTreeNodeModel(
            root,
            tree,
            type,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());

        // Assert
        sut.SystemBaseType.Should().Be(type);
        sut.Uuid.Should().Be(SystemBaseTreeNodeModel.GetUuidByType(type));
        sut.Name.Should().NotBeNullOrWhiteSpace();
        sut.CustomCode.Should().NotBeNullOrWhiteSpace();
        tree.ContentNodes.Should().ContainSingle(x => x.SystemBaseType == type);
    }

    [Fact]
    public void EnsureSystemBaseTypes_CreatesBoolValues()
    {
        // Arrange
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        _ = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());

        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var method = typeof(PhiladelphusRepositoryService).GetMethod(
            "EnsureSystemBaseTypes",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method!.Invoke(service, new object[] { tree });

        // Assert
        var boolNode = tree.ContentNodes
            .OfType<SystemBaseTreeNodeModel>()
            .Single(x => x.SystemBaseType == SystemBaseType.BOOL);

        var boolLeaves = tree.ContentLeaves
            .OfType<SystemBaseTreeLeaveModel>()
            .Where(x => x.ParentNode?.Uuid == boolNode.Uuid)
            .ToList();

        boolLeaves
            .Select(x => x.StringValue)
            .Should()
            .BeEquivalentTo("Истина", "Ложь");

        boolLeaves.Should().ContainSingle(x =>
            x.StringValue == "Истина"
            && x.CustomCode == "TRUE"
            && x.Alias == "tru");

        boolLeaves.Should().ContainSingle(x =>
            x.StringValue == "Ложь"
            && x.CustomCode == "FALS"
            && x.Alias == "fls");
    }

    public static IEnumerable<object[]> SystemBaseTypes()
    {
        return Enum.GetValues<SystemBaseType>()
            .Where(x => x != SystemBaseType.USER_DEFINED)
            .Select(x => new object[] { x });
    }
}
