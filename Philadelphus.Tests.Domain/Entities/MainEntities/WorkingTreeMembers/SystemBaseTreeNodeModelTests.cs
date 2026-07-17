using FluentAssertions;
using AutoMapper;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
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

        var trueLeave = boolLeaves.Single(x => x.StringValue == "Истина");
        notificationService.Messages.Clear();

        trueLeave.StringValue = "false";
        trueLeave.Name = "false";

        trueLeave.StringValue.Should().Be("Истина");
        trueLeave.Name.Should().Be("Истина");
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Изменение системного логического значения")
            && x.Contains("Истина"));
    }

    [Theory]
    [InlineData(SystemBaseType.INTEGER)]
    [InlineData(SystemBaseType.STRING)]
    [InlineData(SystemBaseType.OBJECT)]
    [InlineData(SystemBaseType.NUMERIC)]
    [InlineData(SystemBaseType.FLOAT)]
    [InlineData(SystemBaseType.MONEY)]
    [InlineData(SystemBaseType.DATETIME)]
    [InlineData(SystemBaseType.DATE)]
    [InlineData(SystemBaseType.TIME)]
    public void CreateTreeLeave_SystemBaseNode_LeavesValueEmpty(SystemBaseType type)
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
        var systemBaseLeave = (SystemBaseTreeLeaveModel)leave;
        systemBaseLeave.StringValue.Should().Be(TreeLeaveModel.EmptyStringValue);
        systemBaseLeave.TypedValue.Should().BeNull();
        leave.Name.Should().Be(TreeLeaveModel.EmptyStringValue);
    }

    [Fact]
    public void SystemBaseTreeLeave_NameChange_DoesNotChangeNameOrStringValue()
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

        leave.Name = "abc";

        leave.Name.Should().Be(TreeLeaveModel.EmptyStringValue);
        leave.StringValue.Should().Be(TreeLeaveModel.EmptyStringValue);
        leave.TypedValue.Should().BeNull();
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
        leave.TypedValue.Should().Be("not a typed scalar");
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

        leave.StringValue.Should().Be(TreeLeaveModel.EmptyStringValue);
        notificationService.Messages.Should().Contain(x =>
            x.Contains("not integer")
            && x.Contains(SystemBaseType.INTEGER.ToString())
            && x.Contains("Int64"));
    }

    [Theory]
    [InlineData(SystemBaseType.DATETIME, "2026-05-23T14:15:16+03:00", typeof(DateTimeOffset))]
    [InlineData(SystemBaseType.DATE, "2026-05-23", typeof(DateOnly))]
    [InlineData(SystemBaseType.TIME, "14:15:16", typeof(TimeOnly))]
    public void SystemBaseTreeLeave_TemporalStringValue_AllowsOnlyExplicitInvariantFormat(
        SystemBaseType type,
        string validValue,
        Type expectedTypedValueType)
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
        var leave = (SystemBaseTreeLeaveModel)service.CreateTreeLeave(node);

        leave.StringValue = validValue;
        var storedValue = leave.StringValue;
        leave.StringValue = GetNonInvariantTemporalValue(type);

        leave.StringValue.Should().Be(storedValue);
        leave.TypedValue.Should().BeOfType(expectedTypedValueType);
        notificationService.Messages.Should().Contain(x =>
            x.Contains(GetNonInvariantTemporalValue(type))
            && x.Contains("invariant culture"));
    }

    [Fact]
    public void SystemBaseTreeLeave_FileValue_AllowsMissingLocalFileReference()
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
        notificationService.Messages.Clear();

        leave.StringValue = missingFilePath;

        leave.StringValue.Should().Be(missingFilePath);
        leave.TypedValue.Should().Be(missingFilePath);
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SystemBaseTreeLeave_FileValue_AllowsSupportedReferenceFormats()
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
        var localPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.missing");
        var fileUri = new Uri(localPath).AbsoluteUri;
        var externalUri = $"minio://bucket/{Guid.NewGuid():N}";
        notificationService.Messages.Clear();

        leave.StringValue = localPath;
        leave.StringValue.Should().Be(localPath);

        leave.StringValue = fileUri;
        leave.StringValue.Should().Be(fileUri);

        leave.StringValue = externalUri;
        leave.StringValue.Should().Be(externalUri);
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SystemBaseTreeLeave_FileValue_BlocksUnsupportedReferenceFormat()
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

        leave.StringValue = "relative-file-name.txt";

        leave.StringValue.Should().Be(TreeLeaveModel.EmptyStringValue);
        notificationService.Messages.Should().Contain(x =>
            x.Contains("relative-file-name.txt")
            && x.Contains("FILE")
            && x.Contains("ссылка на файл"));
    }

    [Fact]
    public void SystemBaseTreeLeave_FileValue_AllowsExistingLocalFileAsTypedValue()
    {
        var filePath = Path.GetTempFileName();

        try
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
            notificationService.Messages.Clear();

            leave.StringValue = filePath;

            leave.StringValue.Should().Be(filePath);
            leave.TypedValue.Should().Be(filePath);
            notificationService.Messages.Should().BeEmpty();
        }
        finally
        {
            File.Delete(filePath);
        }
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

    [Theory]
    [InlineData(ProtectedSystemElement.WorkingTree)]
    [InlineData(ProtectedSystemElement.Root)]
    [InlineData(ProtectedSystemElement.Node)]
    [InlineData(ProtectedSystemElement.Leave)]
    public void SoftDeleteShrubMember_SystemElement_BlocksDeleting(
        ProtectedSystemElement elementKind)
    {
        var notificationService = new FakeNotificationService();
        var tree = new TestSystemBaseWorkingTreeModel(notificationService);
        var root = new TreeRootModel(
            TreeRootModel.SystemBaseUuid,
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
        var element = elementKind switch
        {
            ProtectedSystemElement.WorkingTree => (IContentModel)tree,
            ProtectedSystemElement.Root => root,
            ProtectedSystemElement.Node => node,
            ProtectedSystemElement.Leave => leave,
            _ => throw new ArgumentOutOfRangeException(nameof(elementKind)),
        };
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var result = service.SoftDeleteShrubMember(element);

        result.Should().BeFalse();
        ((IMainEntityWritableModel)element).State.Should().NotBe(State.ForSoftDelete);
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Удаление")
            && x.Contains("запрещено"));
    }

    [Fact]
    public void SoftDeleteShrubMember_UserNodeInheritedFromSystemNode_AllowsDeleting()
    {
        var notificationService = new FakeNotificationService();
        var tree = new TestSystemBaseWorkingTreeModel(notificationService);
        var root = new TreeRootModel(
            TreeRootModel.SystemBaseUuid,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var systemNode = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.INTEGER,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var userNode = new TreeNodeModel(
            Guid.NewGuid(),
            systemNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        ((IMainEntityWritableModel)userNode).SetState(State.SavedOrLoaded);

        var result = service.SoftDeleteShrubMember(userNode);

        result.Should().BeTrue();
        userNode.State.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void SoftDeleteShrubMember_UserValueWithSystemType_BlocksDeleting()
    {
        var notificationService = new FakeNotificationService();
        var tree = new TestSystemBaseWorkingTreeModel(notificationService);
        var root = new TreeRootModel(
            TreeRootModel.SystemBaseUuid,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var systemNode = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.INTEGER,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var userValue = new SystemBaseTreeLeaveModel(
            Guid.NewGuid(),
            systemNode,
            tree,
            SystemBaseType.INTEGER,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);

        var result = service.SoftDeleteShrubMember(userValue);

        result.Should().BeFalse();
        userValue.State.Should().NotBe(State.ForSoftDelete);
    }

    [Fact]
    public void SoftDeleteShrubMember_UserLeave_AllowsDeleting()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var userLeave = new TreeLeaveModel(
            Guid.NewGuid(),
            node,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        ((IMainEntityWritableModel)userLeave).SetState(State.SavedOrLoaded);

        var result = service.SoftDeleteShrubMember(userLeave);

        result.Should().BeTrue();
        userLeave.State.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void SoftDeleteShrubMember_AttributeWithInheritedDescendants_BlocksDeleting()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var attribute = CreateAttribute(root, tree, notificationService);
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        ((IMainEntityWritableModel)attribute).SetState(State.SavedOrLoaded);

        node.ParentElementAttributes.Should().ContainSingle(x => x.DeclaringUuid == attribute.DeclaringUuid);

        var result = service.SoftDeleteShrubMember(attribute);

        result.Should().BeFalse();
        attribute.State.Should().Be(State.SavedOrLoaded);
        notificationService.Messages.Should().Contain(x =>
            x.Contains("Удаление атрибута")
            && x.Contains("унаследован"));
    }

    [Fact]
    public void SoftDeleteShrubMember_AttributeWithoutInheritedDescendants_AllowsDeleting()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var attribute = CreateAttribute(root, tree, notificationService);
        var service = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(),
            Mock.Of<ILogger>(),
            notificationService);
        ((IMainEntityWritableModel)attribute).SetState(State.SavedOrLoaded);

        var result = service.SoftDeleteShrubMember(attribute);

        result.Should().BeTrue();
        attribute.State.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void SystemBaseBoolNode_BlocksDirectLeafCollectionEditing()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var boolNode = new SystemBaseTreeNodeModel(
            root,
            tree,
            SystemBaseType.BOOL,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var userNode = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var userLeave = new TreeLeaveModel(
            Guid.NewGuid(),
            userNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var trueLeave = new SystemBaseTreeLeaveModel(
            boolNode,
            tree,
            "\u0418\u0441\u0442\u0438\u043d\u0430",
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        boolNode.AddChild(userLeave).Should().BeFalse();
        boolNode.AddChild(trueLeave).Should().BeFalse();
        boolNode.RemoveChild(trueLeave).Should().BeFalse();
        boolNode.ClearChilds().Should().BeFalse();

        boolNode.ChildLeaves.Should().ContainSingle(x => x == trueLeave);
        boolNode.ChildLeaves.Should().NotContain(userLeave);
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

    [Fact]
    public void SystemBaseTreeNode_BlocksAttributeEditing()
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
        var attribute = CreateAttribute(tree, notificationService);

        node.AddAttribute(attribute).Should().BeFalse();
        node.RemoveAttribute(attribute).Should().BeFalse();
        node.ClearAttributes().Should().BeFalse();
    }

    [Fact]
    public void SystemBaseWorkingTree_BlocksAttributeEditing()
    {
        var notificationService = new FakeNotificationService();
        var tree = new TestSystemBaseWorkingTreeModel(notificationService);
        var attribute = CreateAttribute(tree, notificationService);

        tree.AddAttribute(attribute).Should().BeFalse();
        tree.RemoveAttribute(attribute).Should().BeFalse();
        tree.ClearAttributes().Should().BeFalse();
    }

    [Fact]
    public void SystemBaseTreeRoot_BlocksAttributeEditing()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            TreeRootModel.SystemBaseUuid,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var attribute = CreateAttribute(tree, notificationService);

        root.AddAttribute(attribute).Should().BeFalse();
        root.RemoveAttribute(attribute).Should().BeFalse();
        root.ClearAttributes().Should().BeFalse();
    }

    [Fact]
    public void TreeLeave_BlocksAttributeEditing()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var leave = new TreeLeaveModel(
            Guid.NewGuid(),
            node,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var attribute = CreateAttribute(tree, notificationService);

        leave.AddAttribute(attribute).Should().BeFalse();
        leave.RemoveAttribute(attribute).Should().BeFalse();
        leave.ClearAttributes().Should().BeFalse();
    }

    [Fact]
    public void SystemBaseTreeLeave_BlocksAttributeEditing()
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
        var leave = new SystemBaseTreeLeaveModel(
            Guid.NewGuid(),
            node,
            tree,
            SystemBaseType.INTEGER,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var attribute = CreateAttribute(tree, notificationService);

        leave.AddAttribute(attribute).Should().BeFalse();
        leave.RemoveAttribute(attribute).Should().BeFalse();
        leave.ClearAttributes().Should().BeFalse();
    }

    public static IEnumerable<object[]> SystemBaseTypes()
    {
        return Enum.GetValues<SystemBaseType>()
            .Where(x => x != SystemBaseType.USER_DEFINED)
            .Select(x => new object[] { x });
    }

    private static ElementAttributeModel CreateAttribute(
        WorkingTreeModel owner,
        FakeNotificationService notificationService)
    {
        var uuid = Guid.NewGuid();

        return new ElementAttributeModel(
            uuid,
            owner,
            uuid,
            owner,
            owner,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>());
    }

    private static ElementAttributeModel CreateAttribute(
        IAttributeOwnerModel owner,
        WorkingTreeModel tree,
        FakeNotificationService notificationService)
    {
        var uuid = Guid.NewGuid();

        return new ElementAttributeModel(
            uuid,
            owner,
            uuid,
            owner,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>());
    }

    private sealed class TestSystemBaseWorkingTreeModel : WorkingTreeModel
    {
        public TestSystemBaseWorkingTreeModel(FakeNotificationService notificationService)
            : base(
                WorkingTreeModel.SystemBaseUuid,
                new FakeDataStorageModel(),
                new FakeShrubModel(),
                notificationService,
                new EmptyPropertiesPolicy<WorkingTreeModel>())
        {
        }
    }

    public enum ProtectedSystemElement
    {
        WorkingTree,
        Root,
        Node,
        Leave,
    }

    private static string GetNonInvariantTemporalValue(SystemBaseType type)
    {
        return type switch
        {
            SystemBaseType.DATETIME => "23.05.2026 14:15:16 +03:00",
            SystemBaseType.DATE => "23.05.2026",
            SystemBaseType.TIME => "2:15 PM",
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }
}
