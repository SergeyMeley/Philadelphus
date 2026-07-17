using FluentAssertions;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.LeaveAttributeValues.Services;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.LeaveAttributeValues;

/// <summary>
/// Проверяет публичные операции сервиса значений атрибутов листов.
/// </summary>
public class LeaveAttributeValueServiceTests
{
    [Fact]
    public void FindMatches_UsesOnlyRequestedDeclarations()
    {
        var graph = CreateGraph();
        var comparedDeclaration = CreateDeclaration(graph);
        var ignoredDeclaration = CreateDeclaration(graph);
        var expected = CreateLeave(graph);
        var matching = CreateLeave(graph);
        var different = CreateLeave(graph);

        SetValue(expected, comparedDeclaration, graph.FirstValue);
        SetValue(matching, comparedDeclaration, graph.FirstValue);
        SetValue(different, comparedDeclaration, graph.SecondValue);
        SetValue(expected, ignoredDeclaration, graph.FirstValue);
        SetValue(matching, ignoredDeclaration, graph.SecondValue);

        var result = new LeaveAttributeValueService().FindMatches(
            [GetAttribute(expected, comparedDeclaration)],
            [different, matching]);

        result.IsValid.Should().BeTrue();
        result.Matches.Should().Equal(matching);
    }

    [Fact]
    public void FindMatches_IsInvalidWhenCandidateValueCannotBeResolved()
    {
        var graph = CreateGraph();
        var declaration = CreateDeclaration(graph);
        var expected = CreateLeave(graph);
        var candidate = CreateLeave(graph);
        SetValue(expected, declaration, graph.FirstValue);
        SetValue(candidate, declaration, graph.FirstValue);
        GetAttribute(candidate, declaration).ValueFormulaErrorCode = "FORMULA_ERROR";

        var result = new LeaveAttributeValueService().FindMatches(
            [GetAttribute(expected, declaration)],
            [candidate]);

        result.IsValid.Should().BeFalse();
        result.Matches.Should().BeEmpty();
    }

    private static ElementAttributeModel CreateDeclaration(TestGraph graph)
    {
        var uuid = Guid.NewGuid();
        return new(uuid, graph.CandidateNode, uuid, graph.CandidateNode, graph.Tree,
            graph.NotificationService, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = graph.ValueType
        };
    }

    private static TreeLeaveModel CreateLeave(TestGraph graph) =>
        new(Guid.NewGuid(), graph.CandidateNode, graph.Tree, graph.NotificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

    private static ElementAttributeModel GetAttribute(
        TreeLeaveModel leave,
        ElementAttributeModel declaration) =>
        leave.Attributes.Single(x => x.DeclaringUuid == declaration.DeclaringUuid);

    private static void SetValue(
        TreeLeaveModel leave,
        ElementAttributeModel declaration,
        TreeLeaveModel value) =>
        GetAttribute(leave, declaration).Value = value;

    private static TestGraph CreateGraph()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var candidateNode = CreateNode(root, tree, notificationService);
        var valueType = CreateNode(root, tree, notificationService);

        return new(tree, candidateNode, valueType,
            new(Guid.NewGuid(), valueType, tree, notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>()),
            new(Guid.NewGuid(), valueType, tree, notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>()),
            notificationService);
    }

    private static TreeNodeModel CreateNode(
        IParentModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(Guid.NewGuid(), parent, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());

    private sealed record TestGraph(
        WorkingTreeModel Tree,
        TreeNodeModel CandidateNode,
        TreeNodeModel ValueType,
        TreeLeaveModel FirstValue,
        TreeLeaveModel SecondValue,
        FakeNotificationService NotificationService);
}
