using FluentAssertions;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

using System.Collections.ObjectModel;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.WorkingTreeMembers;

public class RecursiveRelationshipTests
{
    [Fact]
    public void AllParentsRecursive_Node_ReturnsParentsToRoot()
    {
        var graph = CreateGraph();

        graph.GrandChild.AllParentsRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Equal(graph.Child.Uuid, graph.Root.Uuid);
    }

    [Fact]
    public void AllParentsRecursive_Leave_ReturnsParentsToRoot()
    {
        var graph = CreateGraph();

        graph.Leave.AllParentsRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Equal(graph.GrandChild.Uuid, graph.Child.Uuid, graph.Root.Uuid);
    }

    [Fact]
    public void AllChildsRecursive_RootAndNode_ReturnRecursiveChildren()
    {
        var graph = CreateGraph();

        graph.Root.AllChildsRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Equal(graph.Child.Uuid, graph.GrandChild.Uuid, graph.Leave.Uuid);

        graph.Child.AllChildsRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Equal(graph.GrandChild.Uuid, graph.Leave.Uuid);
    }

    [Fact]
    public void AllContentRecursive_WorkingTree_ReturnsTreeContentRecursively()
    {
        var graph = CreateGraph();

        graph.WorkingTree.AllContentRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Contain([graph.Root.Uuid, graph.Child.Uuid, graph.GrandChild.Uuid, graph.Leave.Uuid]);
    }

    [Fact]
    public void AllContentRecursive_Node_ReturnsAttributeContent()
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

        graph.Child.AllContentRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .Contain(attribute.Uuid);
    }

    [Fact]
    public void AllOwnersRecursive_Node_ReturnsWorkingTreeAndShrub()
    {
        var graph = CreateGraph();

        graph.Child.AllOwnersRecursive.Values
            .Select(x => x.Uuid)
            .Should()
            .StartWith([graph.WorkingTree.Uuid, graph.WorkingTree.OwningShrub.Uuid]);
    }

    [Fact]
    public void EnumerateChildsRecursive_StopsOnRepeatedUuid()
    {
        var node = new LoopParent();
        node.AddChild(node);

        RecursiveRelationshipHelper.EnumerateChildsRecursive(node)
            .Should()
            .BeEmpty();
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
        var grandChild = new TreeNodeModel(
            Guid.NewGuid(),
            child,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var leave = new TreeLeaveModel(
            Guid.NewGuid(),
            grandChild,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        return new TestGraph(tree, root, child, grandChild, leave);
    }

    private sealed record TestGraph(
        FakeWorkingTreeModel WorkingTree,
        TreeRootModel Root,
        TreeNodeModel Child,
        TreeNodeModel GrandChild,
        TreeLeaveModel Leave);

    private sealed class LoopParent : IParentModel, IChildrenModel
    {
        private readonly Dictionary<Guid, IChildrenModel> _childs = new();

        public Guid Uuid { get; } = Guid.NewGuid();

        public IParentModel Parent => this;

        public ReadOnlyDictionary<Guid, IParentModel> AllParentsRecursive => new(new Dictionary<Guid, IParentModel>());

        public ReadOnlyDictionary<Guid, IChildrenModel> Childs => new(_childs);

        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive => RecursiveRelationshipHelper.ToReadOnlyDictionary(
            RecursiveRelationshipHelper.EnumerateChildsRecursive(this));

        public bool AddChild(IChildrenModel child)
        {
            _childs[child.Uuid] = child;
            return true;
        }

        public bool RemoveChild(IChildrenModel child) => _childs.Remove(child.Uuid);

        public bool ClearChilds()
        {
            _childs.Clear();
            return true;
        }

        public bool ChangeParent(IParentModel newParent) => false;
    }
}
