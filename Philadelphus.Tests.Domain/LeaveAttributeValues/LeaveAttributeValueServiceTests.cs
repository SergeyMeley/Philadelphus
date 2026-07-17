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

    [Fact]
    public void FillFromLeave_ReplacesScalarWithReferenceAndClearsEmptyValue()
    {
        var graph = CreateGraph();
        var replacedDeclaration = CreateDeclaration(graph);
        var clearedDeclaration = CreateDeclaration(graph);
        var source = CreateLeave(graph);
        var target = CreateLeave(graph);
        SetValue(source, replacedDeclaration, graph.SecondValue);
        SetValue(target, replacedDeclaration, graph.FirstValue);
        SetValue(target, clearedDeclaration, graph.FirstValue);

        var result = new LeaveAttributeValueService().FillFromLeave(
            target, source, [replacedDeclaration.DeclaringUuid, clearedDeclaration.DeclaringUuid]);

        var replaced = GetAttribute(target, replacedDeclaration);
        var cleared = GetAttribute(target, clearedDeclaration);
        result.ChangedAttributes.Should().Equal(replaced, cleared);
        replaced.Value.Should().BeSameAs(graph.SecondValue);
        replaced.ValueFormula.Should().Be($"=[{graph.SecondValue.Uuid}]");
        cleared.Value.Should().BeNull();
        cleared.ValueFormula.Should().BeEmpty();
    }

    [Fact]
    public void FillFromLeave_ReplacesAndClearsCollectionsAndIgnoresOrder()
    {
        var graph = CreateGraph();
        var replacedDeclaration = CreateDeclaration(graph, isCollection: true);
        var clearedDeclaration = CreateDeclaration(graph, isCollection: true);
        var unchangedDeclaration = CreateDeclaration(graph, isCollection: true);
        var source = CreateLeave(graph);
        var target = CreateLeave(graph);
        AddValues(target, replacedDeclaration, graph.FirstValue);
        AddValues(source, replacedDeclaration, graph.SecondValue);
        AddValues(target, clearedDeclaration, graph.FirstValue);
        AddValues(target, unchangedDeclaration, graph.FirstValue, graph.SecondValue);
        AddValues(source, unchangedDeclaration, graph.SecondValue, graph.FirstValue);

        var service = new LeaveAttributeValueService();
        var result = service.FillFromLeave(target, source,
            [replacedDeclaration.DeclaringUuid, clearedDeclaration.DeclaringUuid,
                unchangedDeclaration.DeclaringUuid]);

        var replaced = GetAttribute(target, replacedDeclaration);
        var cleared = GetAttribute(target, clearedDeclaration);
        result.ChangedAttributes.Should().Equal(replaced, cleared);
        replaced.Values.Should().Equal(graph.SecondValue);
        cleared.Values.Should().BeEmpty();
        service.FillFromLeave(target, source,
                [replacedDeclaration.DeclaringUuid, clearedDeclaration.DeclaringUuid])
            .ChangedAttributes.Should().BeEmpty();
    }

    private static ElementAttributeModel CreateDeclaration(
        TestGraph graph,
        bool isCollection = false)
    {
        var uuid = Guid.NewGuid();
        return new(uuid, graph.CandidateNode, uuid, graph.CandidateNode, graph.Tree,
            graph.NotificationService, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = graph.ValueType,
            IsCollectionValue = isCollection
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

    private static void AddValues(
        TreeLeaveModel leave,
        ElementAttributeModel declaration,
        params TreeLeaveModel[] values)
    {
        foreach (var value in values)
            GetAttribute(leave, declaration).TryAddValueToValuesCollection(value).Should().BeTrue();
    }

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
