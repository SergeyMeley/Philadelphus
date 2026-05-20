using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

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

    public static IEnumerable<object[]> SystemBaseTypes()
    {
        return Enum.GetValues<SystemBaseType>()
            .Where(x => x != SystemBaseType.USER_DEFINED)
            .Select(x => new object[] { x });
    }
}
