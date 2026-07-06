using FluentAssertions;

using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Presentation.Services.StateVisibility;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.Services.StateVisibility;

public class StateVisibilityInfoBuilderTests
{
    [Fact]
    public void Build_SelectsHighestPriorityState()
    {
        var graph = CreateGraph();
        SetState(graph.Child, State.Changed);
        SetState(graph.Leave, State.ForHardDelete);

        var result = StateVisibilityInfoBuilder.Build(graph.Root);

        result.ChildContentState.Should().Be(State.ForHardDelete);
    }

    [Fact]
    public void Build_ParentStateGoesToUpperSegment()
    {
        var graph = CreateGraph();
        SetState(graph.Root, State.ForSoftDelete);

        var result = StateVisibilityInfoBuilder.Build(graph.Child);

        result.ParentOwnerState.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void Build_ChildAndContentStatesGoToLowerSegment()
    {
        var graph = CreateGraph();
        var attributeUuid = Guid.NewGuid();
        var attribute = new ElementAttributeModel(
            attributeUuid,
            graph.Child,
            attributeUuid,
            graph.Child,
            graph.WorkingTree,
            new FakeNotificationService(),
            new EmptyPropertiesPolicy<ElementAttributeModel>());
        SetState(attribute, State.Changed);
        SetState(graph.Leave, State.ForSoftDelete);

        var result = StateVisibilityInfoBuilder.Build(graph.Child);

        result.ChildContentState.Should().Be(State.ForSoftDelete);
    }

    [Fact]
    public void Build_ToolTipUsesEmptyForApplicableEmptyGroups()
    {
        var graph = CreateGraph();

        var result = StateVisibilityInfoBuilder.Build(graph.Leave);

        result.ToolTip.Should().Contain("Наследники: <не применимо>");
        result.ToolTip.Should().Contain("Содержимое: <пусто>");
        result.ToolTip.Should().NotContain(":\r\n");
    }

    [Fact]
    public void Build_ToolTipUsesNotApplicableForUnsupportedGroups()
    {
        var graph = CreateGraph();

        var result = StateVisibilityInfoBuilder.Build(graph.Root);

        result.ToolTip.Should().Contain("Родители: <не применимо>");
        result.ToolTip.Should().Contain("Владелец:");
        result.ToolTip.Should().NotContain("Владелец: <не применимо>");
        result.ToolTip.Should().NotContain(":\r\n");
    }

    private static TestGraph CreateGraph()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var child = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var leave = new TreeLeaveModel(
            Guid.NewGuid(),
            child,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        return new TestGraph(tree, root, child, leave);
    }

    private static void SetState(IMainEntityWritableModel model, State state)
    {
        model.SetState(State.SavedOrLoaded);
        model.SetState(state);
    }

    private sealed record TestGraph(
        FakeWorkingTreeModel WorkingTree,
        TreeRootModel Root,
        TreeNodeModel Child,
        TreeLeaveModel Leave);
}
