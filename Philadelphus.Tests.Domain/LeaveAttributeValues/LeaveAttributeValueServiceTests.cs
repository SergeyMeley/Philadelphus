using AutoMapper;
using FluentAssertions;
using Moq;

using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.LeaveAttributeValues.Services;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

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

        var result = CreateService().FindMatches(
            [GetAttribute(expected, comparedDeclaration)],
            [different, matching]);

        result.IsValid.Should().BeTrue();
        result.Status.Should().Be(LeaveAttributeMatchStatus.Resolved);
        result.Matches.Should().Equal(matching);
        result.ResolvedMatch.Should().BeSameAs(matching);
    }

    [Theory]
    [InlineData(0, LeaveAttributeMatchStatus.NotFound)]
    [InlineData(1, LeaveAttributeMatchStatus.Resolved)]
    [InlineData(2, LeaveAttributeMatchStatus.Ambiguous)]
    public void FindMatches_ReturnsStatusForMatchCount(
        int matchCount,
        LeaveAttributeMatchStatus expectedStatus)
    {
        var graph = CreateGraph();
        var declaration = CreateDeclaration(graph);
        var expected = CreateLeave(graph);
        SetValue(expected, declaration, graph.FirstValue);
        var candidates = Enumerable.Range(0, matchCount)
            .Select(_ => CreateLeave(graph))
            .ToList();
        foreach (var candidate in candidates)
            SetValue(candidate, declaration, graph.FirstValue);

        var result = CreateService().FindMatches(
            [GetAttribute(expected, declaration)],
            candidates);

        result.Status.Should().Be(expectedStatus);
        result.Matches.Should().Equal(candidates);
        result.ResolvedMatch.Should().BeSameAs(
            matchCount == 1 ? candidates[0] : null);
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

        var result = CreateService().FindMatches(
            [GetAttribute(expected, declaration)],
            [candidate]);

        result.IsValid.Should().BeFalse();
        result.Status.Should().Be(LeaveAttributeMatchStatus.Invalid);
        result.Matches.Should().BeEmpty();
        result.ResolvedMatch.Should().BeNull();
    }

    [Fact]
    public void FindMatches_DraftsCompareCompleteSetAndRejectValueErrors()
    {
        var graph = CreateGraph();
        var scalar = CreateDeclaration(graph);
        var collection = CreateDeclaration(graph, isCollection: true);
        var extra = CreateDeclaration(graph);
        var candidate = CreateLeave(graph);
        AddValues(candidate, collection, graph.SecondValue, graph.FirstValue);
        SetValue(candidate, extra, graph.FirstValue);
        var scalarDraft = LeaveAttributeValueDraft.Scalar(scalar.DeclaringUuid, null);
        var collectionDraft = LeaveAttributeValueDraft.Collection(
            collection.DeclaringUuid,
            [graph.FirstValue.Uuid, graph.SecondValue.Uuid, graph.FirstValue.Uuid]);
        var extraDraft = LeaveAttributeValueDraft.Scalar(
            extra.DeclaringUuid,
            graph.FirstValue.Uuid);
        var service = CreateService();

        var resolved = service.FindMatches(
            [scalarDraft, collectionDraft, extraDraft],
            [candidate]);

        scalarDraft.ValueUuid.Should().BeNull();
        collectionDraft.ValueUuids.Should().BeEquivalentTo(
            [graph.FirstValue.Uuid, graph.SecondValue.Uuid]);
        resolved.Status.Should().Be(LeaveAttributeMatchStatus.Resolved);
        resolved.ResolvedMatch.Should().BeSameAs(candidate);
        service.FindMatches([scalarDraft, collectionDraft], [candidate])
            .Status.Should().Be(LeaveAttributeMatchStatus.NotFound);

        GetAttribute(candidate, extra).ValueFormulaErrorCode = "FORMULA_ERROR";
        service.FindMatches([scalarDraft, collectionDraft, extraDraft], [candidate])
            .Status.Should().Be(LeaveAttributeMatchStatus.Invalid);
        GetAttribute(candidate, extra).ValueFormulaErrorCode = string.Empty;
        GetAttribute(candidate, collection).LoadValues(
            [graph.FirstValue, graph.SecondValue],
            [Guid.NewGuid()]);
        service.FindMatches([scalarDraft, collectionDraft, extraDraft], [candidate])
            .Status.Should().Be(LeaveAttributeMatchStatus.Invalid);
    }

    [Fact]
    public void FindSystemValue_ComparesTypedValuesAndDetectsAmbiguity()
    {
        var graph = CreateGraph();
        var valueType = CreateSystemNode(graph, SystemBaseType.INTEGER);
        var first = CreateSystemLeave(graph, valueType, "1");
        var service = CreateService();

        var resolved = service.FindSystemValue(valueType, "01");

        resolved.Status.Should().Be(LeaveAttributeMatchStatus.Resolved);
        resolved.ResolvedMatch.Should().BeSameAs(first);

        var second = CreateSystemLeave(graph, valueType, "001");
        var ambiguous = service.FindSystemValue(valueType, "1");

        ambiguous.Status.Should().Be(LeaveAttributeMatchStatus.Ambiguous);
        ambiguous.Matches.Should().Equal(first, second);
        ambiguous.ResolvedMatch.Should().BeNull();

        second.AuditInfo.IsDeleted = true;
        service.FindSystemValue(valueType, "1").ResolvedMatch
            .Should().BeSameAs(first);
    }

    [Fact]
    public void FindSystemValue_IsInvalidForWrongFormat()
    {
        var graph = CreateGraph();
        var valueType = CreateSystemNode(graph, SystemBaseType.DATE);

        var result = CreateService().FindSystemValue(valueType, "31.12.2025");

        result.Status.Should().Be(LeaveAttributeMatchStatus.Invalid);
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public void CreateSystemValue_CreatesOnlyForNotFoundValue()
    {
        var graph = CreateGraph();
        var valueType = CreateSystemNode(graph, SystemBaseType.INTEGER);
        var repositoryService = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(), Mock.Of<ILogger>(), graph.NotificationService);
        var service = new LeaveAttributeValueService(repositoryService);

        var created = service.CreateSystemValue(valueType, "01");

        created.ParentNode.Should().BeSameAs(valueType);
        created.StringValue.Should().Be("01");
        created.TypedValue.Should().Be(1L);
        created.State.Should().Be(State.Initialized);
        var duplicateAction = () => service.CreateSystemValue(valueType, "1");
        duplicateAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже представлено*");
    }

    [Fact]
    public void CreateSystemValue_RejectsInvalidAndBoolValues()
    {
        var graph = CreateGraph();
        var repositoryService = new Mock<IPhiladelphusRepositoryService>();
        var service = new LeaveAttributeValueService(repositoryService.Object);
        var integerType = CreateSystemNode(graph, SystemBaseType.INTEGER);
        var boolType = CreateSystemNode(graph, SystemBaseType.BOOL);

        var invalidAction = () => service.CreateSystemValue(integerType, "не число");
        var boolAction = () => service.CreateSystemValue(boolType, "Истина");

        invalidAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*не соответствует системному типу*");
        boolAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*BOOL запрещено*");
        repositoryService.Verify(x => x.CreateTreeLeave(
            It.IsAny<TreeNodeModel>(), true, true), Times.Never);
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

        var result = CreateService().FillFromLeave(
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

        var service = CreateService();
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

    [Fact]
    public void CountFillChanges_DoesNotMutateTargetAndMatchesFillResult()
    {
        var graph = CreateGraph();
        var declaration = CreateDeclaration(graph);
        var source = CreateLeave(graph);
        var target = CreateLeave(graph);
        SetValue(source, declaration, graph.SecondValue);
        SetValue(target, declaration, graph.FirstValue);
        var service = CreateService();

        var changedCount = service.CountFillChanges(
            target,
            source,
            [declaration.DeclaringUuid]);

        changedCount.Should().Be(1);
        GetAttribute(target, declaration).Value.Should().BeSameAs(graph.FirstValue);
        service.FillFromLeave(target, source, [declaration.DeclaringUuid])
            .ChangedAttributes.Should().HaveCount(changedCount);
    }

    [Fact]
    public void CreateLeave_CreatesInitializedAutoNamedLeaveWithSourceValues()
    {
        var graph = CreateGraph();
        var scalarDeclaration = CreateDeclaration(graph);
        var collectionDeclaration = CreateDeclaration(graph, isCollection: true);
        var source = CreateLeave(graph);
        SetValue(source, scalarDeclaration, graph.FirstValue);
        AddValues(source, collectionDeclaration, graph.FirstValue, graph.SecondValue);
        var repositoryService = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(), Mock.Of<ILogger>(), graph.NotificationService);

        var created = new LeaveAttributeValueService(repositoryService).CreateLeave(
            graph.CandidateNode,
            [GetAttribute(source, scalarDeclaration), GetAttribute(source, collectionDeclaration)]);

        created.ParentNode.Should().BeSameAs(graph.CandidateNode);
        created.Name.Should().NotBeNullOrWhiteSpace();
        created.State.Should().Be(State.Initialized);
        GetAttribute(created, scalarDeclaration).Value.Should().BeSameAs(graph.FirstValue);
        GetAttribute(created, scalarDeclaration).ValueFormula
            .Should().Be($"=[{graph.FirstValue.Uuid}]");
        GetAttribute(created, collectionDeclaration).Values
            .Should().Equal(graph.FirstValue, graph.SecondValue);
    }

    [Fact]
    public void CreateLeave_DraftsFillByDeclaringUuidAndRejectDuplicates()
    {
        var graph = CreateGraph();
        var emptyScalar = CreateDeclaration(graph);
        var emptyCollection = CreateDeclaration(graph, isCollection: true);
        var assignedScalar = CreateDeclaration(graph);
        emptyScalar.Value = graph.FirstValue;
        emptyCollection.TryAddValueToValuesCollection(graph.FirstValue).Should().BeTrue();
        var drafts = new[]
        {
            LeaveAttributeValueDraft.Collection(emptyCollection.DeclaringUuid, []),
            LeaveAttributeValueDraft.Scalar(assignedScalar.DeclaringUuid, graph.SecondValue.Uuid),
            LeaveAttributeValueDraft.Scalar(emptyScalar.DeclaringUuid, null),
        };
        var repositoryService = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(), Mock.Of<ILogger>(), graph.NotificationService);
        var service = new LeaveAttributeValueService(repositoryService);

        var created = service.CreateLeave(graph.CandidateNode, drafts);

        created.State.Should().Be(State.Initialized);
        GetAttribute(created, emptyScalar).Value.Should().BeNull();
        GetAttribute(created, emptyScalar).ValueFormula.Should().BeEmpty();
        GetAttribute(created, emptyCollection).Values.Should().BeEmpty();
        GetAttribute(created, assignedScalar).Value.Should().BeSameAs(graph.SecondValue);
        GetAttribute(created, assignedScalar).ValueFormula
            .Should().Be($"=[{graph.SecondValue.Uuid}]");
        service.FindMatches(drafts, [created]).ResolvedMatch.Should().BeSameAs(created);

        var duplicateAction = () => service.CreateLeave(graph.CandidateNode, drafts);
        duplicateAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже представлен*");
        graph.CandidateNode.ChildLeaves.Should().ContainSingle();

        var invalidDrafts = drafts
            .Select(x => x.DeclaringUuid == assignedScalar.DeclaringUuid
                ? LeaveAttributeValueDraft.Scalar(x.DeclaringUuid, Guid.NewGuid())
                : x)
            .ToArray();
        var invalidAction = () => service.CreateLeave(graph.CandidateNode, invalidDrafts);
        invalidAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*не все значения*");
        graph.CandidateNode.ChildLeaves.Should().ContainSingle();
    }

    private static LeaveAttributeValueService CreateService() =>
        new(Mock.Of<IPhiladelphusRepositoryService>());

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

    private static SystemBaseTreeNodeModel CreateSystemNode(
        TestGraph graph,
        SystemBaseType type) =>
        new(graph.CandidateNode.Parent, graph.Tree, type, graph.NotificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());

    private static SystemBaseTreeLeaveModel CreateSystemLeave(
        TestGraph graph,
        SystemBaseTreeNodeModel parent,
        string value)
    {
        var leave = new SystemBaseTreeLeaveModel(
            Guid.NewGuid(), parent, graph.Tree, parent.SystemBaseType,
            graph.NotificationService, new EmptyPropertiesPolicy<TreeLeaveModel>());
        leave.StringValue = value;
        return leave;
    }

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
