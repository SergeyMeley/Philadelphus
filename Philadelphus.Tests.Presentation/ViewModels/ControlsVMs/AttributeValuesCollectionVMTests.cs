using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.ViewModels.ControlsVMs;

public class AttributeValuesCollectionVMTests
{
    [Fact]
    public void Constructor_UsesActiveDirectLeavesAndInitialSelection()
    {
        var graph = CreateGraph();
        graph.Deleted.AuditInfo.IsDeleted = true;
        graph.Attribute.TryAddValueToValuesCollection(graph.First).Should().BeTrue();

        var sut = new AttributeValuesCollectionVM(graph.Attribute);

        sut.Attribute.Should().BeSameAs(graph.Attribute);
        sut.Values.Select(x => x.Value).Should().Equal(graph.First, graph.Second);
        sut.Values.Single(x => x.Value == graph.First).IsSelected.Should().BeTrue();
        sut.Values.Single(x => x.Value == graph.Second).IsSelected.Should().BeFalse();
    }

    [Fact]
    public void Selection_ImmediatelyUpdatesCollectionWithoutDuplicates()
    {
        var graph = CreateGraph();
        var sut = new AttributeValuesCollectionVM(graph.Attribute);
        var item = sut.Values.Single(x => x.Value == graph.Second);

        item.IsSelected = true;
        item.IsSelected = true;

        graph.Attribute.Values.Should().Equal(graph.Second);
        item.IsSelected.Should().BeTrue();

        item.IsSelected = false;

        graph.Attribute.Values.Should().BeEmpty();
        item.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void InheritedCollection_FirstChangeCopiesEffectiveParentValues()
    {
        var graph = CreateGraph();
        graph.Attribute.TryAddValueToValuesCollection(graph.First).Should().BeTrue();
        var child = CreateNode(graph.Owner, graph.Tree, graph.Notifications);
        var inherited = child.Attributes.Single(x => x.DeclaringUuid == graph.Attribute.DeclaringUuid);
        var sut = new AttributeValuesCollectionVM(inherited);

        sut.Values.Single(x => x.Value == graph.Second).IsSelected = true;

        inherited.Values.Should().Equal(graph.First, graph.Second);
        inherited.AreValuesOverridden.Should().BeTrue();
        graph.Attribute.Values.Should().Equal(graph.First);
    }

    [Fact]
    public void SealedCollection_DisablesSelectionWithExplanation()
    {
        var graph = CreateGraph();
        graph.Attribute.Override = OverrideType.Sealed;
        var child = CreateNode(graph.Owner, graph.Tree, graph.Notifications);
        var inherited = child.Attributes.Single(x => x.DeclaringUuid == graph.Attribute.DeclaringUuid);
        var sut = new AttributeValuesCollectionVM(inherited);
        var item = sut.Values.Single(x => x.Value == graph.Second);

        item.IsSelected = true;

        sut.CanSelectValues.Should().BeFalse();
        item.IsEnabled.Should().BeFalse();
        item.ToolTip.Should().Contain("запечатана");
        inherited.Values.Should().BeEmpty();
    }

    private static TestGraph CreateGraph()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var owner = CreateNode(root, tree, notifications);
        var valueType = CreateNode(root, tree, notifications);
        var first = CreateLeave(valueType, tree, notifications, "A");
        var second = CreateLeave(valueType, tree, notifications, "B");
        var deleted = CreateLeave(valueType, tree, notifications, "C");
        var uuid = Guid.NewGuid();
        var attribute = new ElementAttributeModel(uuid, owner, uuid, owner, tree,
            notifications, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = valueType,
            IsCollectionValue = true,
        };
        return new(tree, notifications, owner, attribute, first, second, deleted);
    }

    private static TreeNodeModel CreateNode(
        Philadelphus.Core.Domain.Interfaces.IParentModel parent,
        FakeWorkingTreeModel tree,
        FakeNotificationService notifications) =>
        new(Guid.NewGuid(), parent, tree, notifications, new EmptyPropertiesPolicy<TreeNodeModel>());

    private static TreeLeaveModel CreateLeave(
        TreeNodeModel parent,
        FakeWorkingTreeModel tree,
        FakeNotificationService notifications,
        string name)
    {
        var result = new TreeLeaveModel(Guid.NewGuid(), parent, tree, notifications,
            new EmptyPropertiesPolicy<TreeLeaveModel>()) { Name = name };
        return result;
    }

    private sealed record TestGraph(
        FakeWorkingTreeModel Tree,
        FakeNotificationService Notifications,
        TreeNodeModel Owner,
        ElementAttributeModel Attribute,
        TreeLeaveModel First,
        TreeLeaveModel Second,
        TreeLeaveModel Deleted);
}
