using FluentAssertions;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.WorkingTreeMembers;

public class PolymorphicRelationshipTests
{
    [Fact]
    public void SetPolymorphicParentLeave_UpdatesBothSidesWithoutDuplicates()
    {
        var graph = CreateGraph();

        graph.ChildLeave.SetPolymorphicParentLeave(graph.FirstParentLeave).Should().BeTrue();
        graph.ChildLeave.SetPolymorphicParentLeave(graph.FirstParentLeave).Should().BeFalse();

        graph.ChildLeave.PolymorphicParentLeave.Should().BeSameAs(graph.FirstParentLeave);
        graph.FirstParentLeave.PolymorphicChildLeaves.Should().Equal(graph.ChildLeave);

        graph.ChildLeave.SetPolymorphicParentLeave(graph.SecondParentLeave).Should().BeTrue();

        graph.FirstParentLeave.PolymorphicChildLeaves.Should().BeEmpty();
        graph.SecondParentLeave.PolymorphicChildLeaves.Should().Equal(graph.ChildLeave);

        graph.ChildLeave.SetPolymorphicParentLeave(null).Should().BeTrue();
        graph.ChildLeave.PolymorphicParentLeave.Should().BeNull();
        graph.SecondParentLeave.PolymorphicChildLeaves.Should().BeEmpty();
    }

    [Fact]
    public void SetPolymorphicParentLeave_RejectsSelfAndWrongStructuralParent()
    {
        var graph = CreateGraph();

        graph.ChildLeave.SetPolymorphicParentLeave(graph.ChildLeave).Should().BeFalse();
        graph.ChildLeave.SetPolymorphicParentLeave(graph.WrongParentLeave).Should().BeFalse();

        graph.ChildLeave.PolymorphicParentLeave.Should().BeNull();
        graph.ChildLeave.PolymorphicChildLeaves.Should().BeEmpty();
        graph.WrongParentLeave.PolymorphicChildLeaves.Should().BeEmpty();
    }

    [Fact]
    public void NodePolymorphicParentLeave_AcceptsOnlyLeaveOfDirectParentNode()
    {
        var graph = CreateGraph();

        graph.ChildNode.SetPolymorphicParentLeave(graph.FirstParentLeave).Should().BeTrue();
        graph.ChildNode.SetPolymorphicParentLeave(graph.FirstParentLeave).Should().BeFalse();
        graph.ChildNode.SetPolymorphicParentLeave(graph.WrongParentLeave).Should().BeFalse();

        graph.ChildNode.PolymorphicParentLeave.Should().BeSameAs(graph.FirstParentLeave);
        graph.ChildNode.SetPolymorphicParentLeave(null).Should().BeTrue();
        graph.ChildNode.PolymorphicParentLeave.Should().BeNull();
    }

    private static TestGraph CreateGraph()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(), tree, notificationService, new EmptyPropertiesPolicy<TreeRootModel>());
        var parentNode = CreateNode(root, tree, notificationService);
        var childNode = CreateNode(parentNode, tree, notificationService);
        var wrongNode = CreateNode(root, tree, notificationService);

        return new TestGraph(
            childNode,
            CreateLeave(parentNode, tree, notificationService),
            CreateLeave(parentNode, tree, notificationService),
            CreateLeave(childNode, tree, notificationService),
            CreateLeave(wrongNode, tree, notificationService));
    }

    private static TreeNodeModel CreateNode(
        IParentModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(Guid.NewGuid(), parent, tree, notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());

    private static TreeLeaveModel CreateLeave(
        TreeNodeModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(Guid.NewGuid(), parent, tree, notificationService, new EmptyPropertiesPolicy<TreeLeaveModel>());

    private sealed record TestGraph(
        TreeNodeModel ChildNode,
        TreeLeaveModel FirstParentLeave,
        TreeLeaveModel SecondParentLeave,
        TreeLeaveModel ChildLeave,
        TreeLeaveModel WrongParentLeave);
}
