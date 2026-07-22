using FluentAssertions;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.LeaveAttributeValues;

public class LeaveAttributeValueSignatureTests
{
    [Fact]
    public void Matches_UsesDeclaringUuidAndScalarValueUuid()
    {
        var graph = CreateGraph();
        var declaringUuid = Guid.NewGuid();
        var first = CreateAttribute(graph.FirstOwner, graph, declaringUuid);
        var second = CreateAttribute(graph.SecondOwner, graph, declaringUuid);
        first.Value = graph.FirstValue;
        second.Value = graph.FirstValue;

        CreateSignature(first).Matches(CreateSignature(second)).Should().BeTrue();

        second.Value = graph.SecondValue;
        CreateSignature(first).Matches(CreateSignature(second)).Should().BeFalse();
    }

    [Fact]
    public void Matches_TreatsCollectionValuesAsSet()
    {
        var graph = CreateGraph();
        var declaringUuid = Guid.NewGuid();
        var first = CreateAttribute(graph.FirstOwner, graph, declaringUuid, isCollection: true);
        var second = CreateAttribute(graph.SecondOwner, graph, declaringUuid, isCollection: true);
        first.TryAddValueToValuesCollection(graph.FirstValue).Should().BeTrue();
        first.TryAddValueToValuesCollection(graph.SecondValue).Should().BeTrue();
        second.TryAddValueToValuesCollection(graph.SecondValue).Should().BeTrue();
        second.TryAddValueToValuesCollection(graph.FirstValue).Should().BeTrue();

        CreateSignature(first).Matches(CreateSignature(second)).Should().BeTrue();
    }

    [Fact]
    public void Matches_AcceptsEmptyScalarAndCollectionValues()
    {
        var graph = CreateGraph();
        var scalarUuid = Guid.NewGuid();
        var collectionUuid = Guid.NewGuid();

        var first = LeaveAttributeValueSignature.Create([
            CreateAttribute(graph.FirstOwner, graph, scalarUuid),
            CreateAttribute(graph.FirstOwner, graph, collectionUuid, isCollection: true)]);
        var second = LeaveAttributeValueSignature.Create([
            CreateAttribute(graph.SecondOwner, graph, scalarUuid),
            CreateAttribute(graph.SecondOwner, graph, collectionUuid, isCollection: true)]);

        first.IsValid.Should().BeTrue();
        first.Matches(second).Should().BeTrue();
    }

    [Fact]
    public void Create_IsInvalidForFormulaAndUnresolvedTypeReferences()
    {
        var graph = CreateGraph();
        var formulaError = CreateAttribute(graph.FirstOwner, graph, Guid.NewGuid());
        formulaError.ValueFormulaErrorCode = "FORMULA_ERROR";
        var unresolvedType = CreateAttribute(graph.FirstOwner, graph, Guid.NewGuid());
        unresolvedType.LoadValueType(null, Guid.NewGuid());

        CreateSignature(formulaError).IsValid.Should().BeFalse();
        CreateSignature(unresolvedType).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_IsInvalidForDuplicateDeclaringUuid()
    {
        var graph = CreateGraph();
        var declaringUuid = Guid.NewGuid();

        LeaveAttributeValueSignature.Create([
            CreateAttribute(graph.FirstOwner, graph, declaringUuid),
            CreateAttribute(graph.FirstOwner, graph, declaringUuid)])
            .IsValid.Should().BeFalse();
    }

    private static LeaveAttributeValueSignature CreateSignature(ElementAttributeModel attribute) =>
        LeaveAttributeValueSignature.Create([attribute]);

    private static ElementAttributeModel CreateAttribute(
        TreeNodeModel owner,
        TestGraph graph,
        Guid declaringUuid,
        bool isCollection = false) =>
        new(Guid.NewGuid(), owner, declaringUuid, graph.Root, graph.Tree,
            graph.NotificationService, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = graph.ValueType,
            IsCollectionValue = isCollection
        };

    private static TestGraph CreateGraph()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(), tree, notificationService, new EmptyPropertiesPolicy<TreeRootModel>());
        var firstOwner = CreateNode(root, tree, notificationService);
        var secondOwner = CreateNode(root, tree, notificationService);
        var valueType = CreateNode(root, tree, notificationService);

        return new(tree, root, firstOwner, secondOwner, valueType,
            CreateLeave(valueType, tree, notificationService),
            CreateLeave(valueType, tree, notificationService),
            notificationService);
    }

    private static TreeNodeModel CreateNode(
        TreeRootModel root,
        WorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(Guid.NewGuid(), root, tree, notificationService, new EmptyPropertiesPolicy<TreeNodeModel>());

    private static TreeLeaveModel CreateLeave(
        TreeNodeModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(Guid.NewGuid(), parent, tree, notificationService, new EmptyPropertiesPolicy<TreeLeaveModel>());

    private sealed record TestGraph(
        WorkingTreeModel Tree,
        TreeRootModel Root,
        TreeNodeModel FirstOwner,
        TreeNodeModel SecondOwner,
        TreeNodeModel ValueType,
        TreeLeaveModel FirstValue,
        TreeLeaveModel SecondValue,
        FakeNotificationService NotificationService);
}
