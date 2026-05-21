using FluentAssertions;
using AutoMapper;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Builders;
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

    [Theory]
    [InlineData(SystemBaseType.INTEGER, "0")]
    [InlineData(SystemBaseType.FLOAT, "0.0")]
    [InlineData(SystemBaseType.MONEY, "0.0")]
    [InlineData(SystemBaseType.DATETIME, "1970-01-01T00:00:00+00:00")]
    [InlineData(SystemBaseType.DATE, "1970-01-01")]
    [InlineData(SystemBaseType.TIME, "00:00:00")]
    public void CreateTreeLeave_SystemBaseNode_AppliesDefaultStringValue(SystemBaseType type, string expectedValue)
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            type,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var leave = service.CreateTreeLeave(node);

        leave.Should().BeOfType<SystemBaseTreeLeaveModel>();
        ((SystemBaseTreeLeaveModel)leave).StringValue.Should().Be(expectedValue);
        leave.Name.Should().Be(expectedValue);
    }

    [Fact]
    public void SystemBaseTreeLeave_StringValue_AllowsObjectWithoutFormatRestrictions()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.OBJECT,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        var leave = (SystemBaseTreeLeaveModel)service.CreateTreeLeave(node);
        notificationService.Messages.Clear();

        leave.StringValue = "not a typed scalar";

        leave.StringValue.Should().Be("not a typed scalar");
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SystemBaseTreeLeave_StringValue_BlocksInvalidValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.INTEGER,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        var leave = (SystemBaseTreeLeaveModel)service.CreateTreeLeave(node);

        leave.StringValue = "not integer";

        leave.StringValue.Should().Be("0");
        notificationService.Messages.Should().Contain(x =>
            x.Contains("not integer")
            && x.Contains(SystemBaseType.INTEGER.ToString())
            && x.Contains("Int64"));
    }

    [Fact]
    public void SystemBaseTreeLeave_FileValue_BlocksMissingLocalFile()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.FILE,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        var leave = (SystemBaseTreeLeaveModel)service.CreateTreeLeave(node);
        var missingFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.missing");

        leave.StringValue = missingFilePath;

        leave.StringValue.Should().BeEmpty();
        notificationService.Messages.Should().Contain(x =>
            x.Contains(missingFilePath)
            && x.Contains(SystemBaseType.FILE.ToString())
            && x.Contains("локальному файлу"));
    }

    [Fact]
    public void CreateTreeLeave_BoolSystemBaseNode_BlocksAddingValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.BOOL,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var leave = service.CreateTreeLeave(node);

        leave.Should().BeNull();
        node.ChildLeaves.Should().BeEmpty();
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Создание листа")
            && x.Contains("BOOL"));
    }

    [Fact]
    public void SoftDeleteShrubMember_BoolSystemBaseLeave_BlocksDeletingValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.BOOL,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var leave = new SystemBaseTreeLeaveModel(
            node,
            tree,
            "Истина",
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var result = service.SoftDeleteShrubMember(leave);

        result.Should().BeFalse();
        leave.State.Should().NotBe(State.ForSoftDelete);
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Удаление логического значения")
            && x.Contains("Истина"));
    }

    [Fact]
    public void SystemBaseTreeLeave_BoolValue_BlocksChangingExistingValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.BOOL,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var leave = new SystemBaseTreeLeaveModel(
            node,
            tree,
            "Истина",
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        leave.SetPropertiesPolicy(PropertiesPolicyBuilder.CreateTreeLeaveDefault(notificationService));

        leave.StringValue = "false";
        leave.Name = "false";

        leave.StringValue.Should().Be("Истина");
        leave.Name.Should().Be("Истина");
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Изменение системного логического значения")
            && x.Contains("Истина"));
    }

    public static IEnumerable<object[]> SystemBaseTypes()
    {
        return Enum.GetValues<SystemBaseType>()
            .Where(x => x != SystemBaseType.USER_DEFINED)
            .Select(x => new object[] { x });
    }
}
