using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.Attributes;

public class SystemBaseAttributeValuePropertiesRuleTests
{
    [Theory]
    [InlineData(SystemBaseType.INTEGER, "9223372036854775807")]
    [InlineData(SystemBaseType.NUMERIC, "123.45")]
    [InlineData(SystemBaseType.FLOAT, "-123.45E-2")]
    [InlineData(SystemBaseType.MONEY, "1000.50")]
    [InlineData(SystemBaseType.BOOL, "true")]
    [InlineData(SystemBaseType.BOOL, "Истина")]
    [InlineData(SystemBaseType.DATETIME, "2026-05-21T10:15:30+03:00")]
    [InlineData(SystemBaseType.DATE, "2026-05-21")]
    [InlineData(SystemBaseType.TIME, "10:15:30")]
    [InlineData(SystemBaseType.STRING, "any value")]
    [InlineData(SystemBaseType.STRING, "")]
    [InlineData(SystemBaseType.OBJECT, "any value")]
    [InlineData(SystemBaseType.OBJECT, "")]
    public void CanWrite_Allows_Valid_SystemBaseLeave_StringValue(SystemBaseType type, string value)
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var node = CreateSystemNode(type, tree, root, notificationService);
        var leave = CreateSystemLeave(node, type, value, tree, notificationService);

        attribute.ValueType = node;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

        result.Should().BeTrue();
        notificationService.Messages.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SystemBaseType.INTEGER, "1.5", "Int64")]
    [InlineData(SystemBaseType.FLOAT, "1,5", "invariant culture")]
    [InlineData(SystemBaseType.BOOL, "yes", "true/false")]
    [InlineData(SystemBaseType.DATETIME, "not a date", "DateTimeOffset")]
    [InlineData(SystemBaseType.DATE, "10:15:30", "DateOnly")]
    [InlineData(SystemBaseType.TIME, "2026-05-21", "TimeOnly")]
    public void CanWrite_Blocks_Invalid_SystemBaseLeave_StringValue(
        SystemBaseType type,
        string value,
        string expectedFormatPart)
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var node = CreateSystemNode(type, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, value, tree, notificationService);

        attribute.Name = "Test attribute";
        attribute.ValueType = node;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

        result.Should().BeFalse();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Test attribute")
            .And.Contain(value)
            .And.Contain(type.ToString())
            .And.Contain(expectedFormatPart);
    }

    [Fact]
    public void CanWrite_Allows_File_SystemBaseLeave_WhenLocalFileExists()
    {
        var filePath = Path.GetTempFileName();

        try
        {
            var notificationService = new FakeNotificationService();
            var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
            var tree = CreateTreeWithRoot(notificationService, out var root);
            var attribute = CreateAttribute(tree, notificationService);
            var node = CreateSystemNode(SystemBaseType.FILE, tree, root, notificationService);
            var leave = CreateSystemLeave(node, SystemBaseType.FILE, filePath, tree, notificationService);

            attribute.ValueType = node;

            var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

            result.Should().BeTrue();
            notificationService.Messages.Should().BeEmpty();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void CanWrite_Allows_File_SystemBaseLeave_WhenLocalFileDoesNotExist()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var node = CreateSystemNode(SystemBaseType.FILE, tree, root, notificationService);
        var missingFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.missing");
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, missingFilePath, tree, notificationService);

        attribute.ValueType = node;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

        result.Should().BeTrue();
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void CanWrite_Blocks_Unsupported_SystemBaseType()
    {
        const SystemBaseType type = (SystemBaseType)999;
        var result = SystemBaseStringValueValidator.IsValid(type, "value", out var expectedFormat);

        result.Should().BeFalse();
        expectedFormat.Should().Contain(type.ToString());
    }

    [Fact]
    public void CanWrite_Ignores_Non_Value_Property()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var node = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var leave = CreateSystemLeave(node, SystemBaseType.INTEGER, "0", tree, notificationService);

        attribute.ValueType = node;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Name), leave);

        result.Should().BeTrue();
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void CanWrite_Ignores_Non_SystemBaseValueType()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var userDefinedNode = CreateUserDefinedNode(tree, root, notificationService);
        var systemNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var leave = CreateSystemLeave(systemNode, SystemBaseType.INTEGER, "0", tree, notificationService);

        attribute.ValueType = userDefinedNode;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

        result.Should().BeTrue();
        notificationService.Messages.Should().BeEmpty();
    }

    private static FakeWorkingTreeModel CreateTreeWithRoot(
        FakeNotificationService notificationService,
        out TreeRootModel root)
    {
        var tree = new FakeWorkingTreeModel();
        root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());

        return tree;
    }

    private static ElementAttributeModel CreateAttribute(
        FakeWorkingTreeModel tree,
        FakeNotificationService notificationService)
    {
        var uuid = Guid.NewGuid();
        var attribute = new ElementAttributeModel(
            uuid,
            tree,
            uuid,
            tree,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>());

        attribute.Name = "Attribute";
        return attribute;
    }

    private static SystemBaseTreeNodeModel CreateSystemNode(
        SystemBaseType type,
        FakeWorkingTreeModel tree,
        TreeRootModel root,
        FakeNotificationService notificationService)
    {
        return new SystemBaseTreeNodeModel(
            root,
            tree,
            type,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
    }

    private static TreeNodeModel CreateUserDefinedNode(
        FakeWorkingTreeModel tree,
        TreeRootModel root,
        FakeNotificationService notificationService)
    {
        return new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
    }

    private static SystemBaseTreeLeaveModel CreateSystemLeave(
        SystemBaseTreeNodeModel node,
        SystemBaseType type,
        string value,
        FakeWorkingTreeModel tree,
        FakeNotificationService notificationService)
    {
        var leave = new SystemBaseTreeLeaveModel(
            Guid.NewGuid(),
            node,
            tree,
            type,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        leave.StringValue = value;
        return leave;
    }

}
