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
    [InlineData(SystemBaseType.DATETIME, "not a date", "yyyy-MM-dd'T'HH:mm:sszzz")]
    [InlineData(SystemBaseType.DATE, "10:15:30", "yyyy-MM-dd")]
    [InlineData(SystemBaseType.TIME, "2026-05-21", "HH:mm:ss")]
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
    public void CanWrite_Blocks_File_SystemBaseLeave_WhenReferenceFormatIsUnsupported()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var node = CreateSystemNode(SystemBaseType.FILE, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "relative-file-name.txt", tree, notificationService);

        attribute.Name = "File attribute";
        attribute.ValueType = node;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Value), leave);

        result.Should().BeFalse();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("File attribute")
            .And.Contain("relative-file-name.txt")
            .And.Contain(SystemBaseType.FILE.ToString())
            .And.Contain("ссылка на файл");
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

    [Fact]
    public void TryAddValueToValuesCollection_Blocks_Invalid_SystemBaseLeave_StringValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "not integer", tree, notificationService);

        attribute.Name = "Collection attribute";
        attribute.ValueType = integerNode;
        attribute.IsCollectionValue = true;

        var result = attribute.TryAddValueToValuesCollection(leave);

        result.Should().BeFalse();
        attribute.Values.Should().BeEmpty();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Collection attribute")
            .And.Contain("not integer")
            .And.Contain(SystemBaseType.INTEGER.ToString())
            .And.Contain("Int64");
    }

    [Fact]
    public void TryAddValueToValuesCollection_Allows_Valid_SystemBaseLeave_StringValue()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "42", tree, notificationService);

        attribute.ValueType = integerNode;
        attribute.IsCollectionValue = true;

        var result = attribute.TryAddValueToValuesCollection(leave);

        result.Should().BeTrue();
        attribute.Values.Should().ContainSingle(x => x == leave);
        notificationService.Messages.Should().BeEmpty();
    }

    [Fact]
    public void TrySetSystemBaseValueFromString_Uses_Existing_SystemBaseLeave()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var existing = CreateSystemLeave(stringNode, SystemBaseType.STRING, "Text", tree, notificationService);

        attribute.ValueType = stringNode;

        var result = attribute.TrySetSystemBaseValueFromString("Text");

        result.Should().BeTrue();
        attribute.Value.Should().BeSameAs(existing);
        stringNode.ChildLeaves.OfType<SystemBaseTreeLeaveModel>()
            .Count(x => x.StringValue == "Text")
            .Should().Be(1);
    }

    [Fact]
    public void TrySetSystemBaseValueFromString_Creates_New_SystemBaseLeave()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);

        attribute.ValueType = integerNode;

        var result = attribute.TrySetSystemBaseValueFromString("42");

        result.Should().BeTrue();
        attribute.Value.Should().BeOfType<SystemBaseTreeLeaveModel>();
        attribute.Value.ParentNode.Should().Be(integerNode);
        ((SystemBaseTreeLeaveModel)attribute.Value).StringValue.Should().Be("42");
        integerNode.ChildLeaves.OfType<SystemBaseTreeLeaveModel>()
            .Count(x => x.StringValue == "42")
            .Should().Be(1);
        notificationService.Messages.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TrySetSystemBaseValueFromString_Empty_Input_Does_Not_Create_Leave(string? value)
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);

        attribute.ValueType = stringNode;

        var result = attribute.TrySetSystemBaseValueFromString(value);

        result.Should().BeFalse();
        stringNode.ChildLeaves.Should().BeEmpty();
    }

    [Fact]
    public void TrySetSystemBaseValueFromString_Invalid_Input_Does_Not_Create_Leave()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);

        attribute.Name = "Integer attribute";
        attribute.ValueType = integerNode;

        var result = attribute.TrySetSystemBaseValueFromString("not integer");

        result.Should().BeFalse();
        integerNode.ChildLeaves.Should().BeEmpty();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Integer attribute")
            .And.Contain("not integer")
            .And.Contain(SystemBaseType.INTEGER.ToString());
    }

    [Fact]
    public void TrySetSystemBaseValueFromString_Non_SystemBase_ValueType_Does_Not_Change_Value()
    {
        var notificationService = new FakeNotificationService();
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var userDefinedNode = CreateUserDefinedNode(tree, root, notificationService);
        var value = new TreeLeaveModel(
            Guid.NewGuid(),
            userDefinedNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        attribute.ValueType = userDefinedNode;
        attribute.Value = value;

        var result = attribute.TrySetSystemBaseValueFromString("Text");

        result.Should().BeFalse();
        attribute.Value.Should().BeSameAs(value);
        userDefinedNode.ChildLeaves.Should().ContainSingle().Which.Should().BeSameAs(value);
    }

    [Fact]
    public void CanWrite_Blocks_ValueType_Change_When_Current_Value_Becomes_Incompatible()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var dateNode = CreateSystemNode(SystemBaseType.DATE, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "42", tree, notificationService);

        attribute.Name = "Typed attribute";
        attribute.ValueType = integerNode;
        attribute.Value = leave;

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.ValueType), dateNode);

        result.Should().BeFalse();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Typed attribute")
            .And.Contain("42")
            .And.Contain(SystemBaseType.DATE.ToString())
            .And.Contain("yyyy-MM-dd");
    }

    [Fact]
    public void CanWrite_Blocks_ValueType_Change_When_Current_Collection_Value_Becomes_Incompatible()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var dateNode = CreateSystemNode(SystemBaseType.DATE, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "42", tree, notificationService);

        attribute.Name = "Collection attribute";
        attribute.ValueType = integerNode;
        attribute.IsCollectionValue = true;
        attribute.TryAddValueToValuesCollection(leave).Should().BeTrue();
        notificationService.Messages.Clear();

        var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.ValueType), dateNode);

        result.Should().BeFalse();
        notificationService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Collection attribute")
            .And.Contain("42")
            .And.Contain(SystemBaseType.DATE.ToString())
            .And.Contain("yyyy-MM-dd");
    }

    [Fact]
    public void OnWrite_ValueType_Change_Remap_Current_SystemBaseValue_To_New_SystemBaseNode()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var numericNode = CreateSystemNode(SystemBaseType.NUMERIC, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "42", tree, notificationService);

        attribute.ValueType = integerNode;
        attribute.Value = leave;

        var canWrite = rule.CanWrite(attribute, nameof(ElementAttributeModel.ValueType), numericNode);
        attribute.ValueType = numericNode;
        rule.OnWrite(attribute, nameof(ElementAttributeModel.ValueType), integerNode, numericNode);

        canWrite.Should().BeTrue();
        attribute.Value.Should().BeOfType<SystemBaseTreeLeaveModel>();
        attribute.Value.Should().NotBeSameAs(leave);
        attribute.Value.ParentNode.Should().Be(numericNode);
        ((SystemBaseTreeLeaveModel)attribute.Value).StringValue.Should().Be("42");
    }

    [Fact]
    public void OnWrite_ValueType_Change_Remap_Current_SystemBaseCollection_To_New_SystemBaseNode()
    {
        var notificationService = new FakeNotificationService();
        var rule = new SystemBaseAttributeValuePropertiesRule(notificationService);
        var tree = CreateTreeWithRoot(notificationService, out var root);
        var attribute = CreateAttribute(tree, notificationService);
        var integerNode = CreateSystemNode(SystemBaseType.INTEGER, tree, root, notificationService);
        var numericNode = CreateSystemNode(SystemBaseType.NUMERIC, tree, root, notificationService);
        var stringNode = CreateSystemNode(SystemBaseType.STRING, tree, root, notificationService);
        var leave = CreateSystemLeave(stringNode, SystemBaseType.STRING, "42", tree, notificationService);

        attribute.ValueType = integerNode;
        attribute.IsCollectionValue = true;
        attribute.TryAddValueToValuesCollection(leave).Should().BeTrue();

        var canWrite = rule.CanWrite(attribute, nameof(ElementAttributeModel.ValueType), numericNode);
        attribute.ValueType = numericNode;
        rule.OnWrite(attribute, nameof(ElementAttributeModel.ValueType), integerNode, numericNode);

        canWrite.Should().BeTrue();
        attribute.Values.Should().ContainSingle();
        var remappedValue = attribute.Values.Single();
        remappedValue.Should().BeOfType<SystemBaseTreeLeaveModel>();
        remappedValue.Should().NotBeSameAs(leave);
        remappedValue.ParentNode.Should().Be(numericNode);
        ((SystemBaseTreeLeaveModel)remappedValue).StringValue.Should().Be("42");
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
