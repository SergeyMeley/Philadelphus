using FluentAssertions;
using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Models.Tables;
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
        sut.Columns.First().ColumnType.Should().Be(ChildCollectionTableColumnType.CheckBox);
        sut.Columns[1].Key.Should().Be("IsSearchMatch");
        sut.Rows.Select(x => x.SourceUuid).Should().Equal(graph.First.Uuid, graph.Second.Uuid);
        sut.Rows.Single(x => x.SourceUuid == graph.First.Uuid)["IsSelected"].Should().Be(true);
        sut.Rows.Should().OnlyContain(x => x["IsSearchMatch"] == null);
    }

    [Fact]
    public void SystemSearch_AutomaticallyUpdatesStatusMatchesAndResolvedRow()
    {
        var graph = CreateGraph();
        var valueType = new SystemBaseTreeNodeModel(
            graph.Owner.Parent,
            graph.Tree,
            SystemBaseType.INTEGER,
            graph.Notifications,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var first = CreateSystemLeave(graph, valueType, "1");
        var second = CreateSystemLeave(graph, valueType, "2");
        graph.Attribute.ValueType = valueType;
        var service = new StubLeaveAttributeValueService(
            (_, value) => value switch
            {
                "1" => new(LeaveAttributeMatchStatus.Resolved, [first]),
                "оба" => new(LeaveAttributeMatchStatus.Ambiguous, [first, second]),
                "нет" => new(LeaveAttributeMatchStatus.NotFound, []),
                _ => new(LeaveAttributeMatchStatus.Invalid, []),
            });
        var sut = new AttributeValuesCollectionVM(
            graph.Attribute,
            service,
            new DefaultRelayCommandFactory());

        sut.ValueLookup!.SystemValue = "1";

        sut.SearchStatus.Should().Be(LeaveAttributeMatchStatus.Resolved);
        sut.SearchMatchCount.Should().Be(1);
        sut.ResolvedSearchMatch.Should().BeSameAs(first);
        sut.ResolvedSearchRow!.SourceUuid.Should().Be(first.Uuid);
        sut.Rows.Single(x => x.SourceUuid == first.Uuid)["IsSearchMatch"].Should().Be(true);
        sut.Rows.Single(x => x.SourceUuid == second.Uuid)["IsSearchMatch"].Should().Be(false);

        sut.ValueLookup.SystemValue = "оба";
        sut.SearchStatus.Should().Be(LeaveAttributeMatchStatus.Ambiguous);
        sut.SearchMatchCount.Should().Be(2);
        sut.ResolvedSearchRow.Should().BeNull();
        sut.Rows.Should().OnlyContain(x => Equals(x["IsSearchMatch"], true));

        sut.ValueLookup.SystemValue = "нет";
        sut.SearchStatus.Should().Be(LeaveAttributeMatchStatus.NotFound);
        sut.Rows.Should().OnlyContain(x => Equals(x["IsSearchMatch"], false));
        graph.Attribute.Values.Should().BeEmpty();
    }

    [Fact]
    public void LeaveValueLookup_SystemSearchCreatesOnlyMissingNonBoolValue()
    {
        var graph = CreateGraph();
        var valueType = new SystemBaseTreeNodeModel(
            graph.Owner.Parent,
            graph.Tree,
            SystemBaseType.INTEGER,
            graph.Notifications,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        SystemBaseTreeLeaveModel? created = null;
        var service = new StubLeaveAttributeValueService(
            (parent, value) => parent.SystemBaseType == SystemBaseType.BOOL
                ? new(LeaveAttributeMatchStatus.NotFound, [])
                : value switch
                {
                    null => new(LeaveAttributeMatchStatus.Invalid, []),
                    "дубль" => new(LeaveAttributeMatchStatus.Ambiguous, [graph.First, graph.Second]),
                    _ when created != null => new(LeaveAttributeMatchStatus.Resolved, [created]),
                    _ => new(LeaveAttributeMatchStatus.NotFound, []),
                },
            createSystemValue: (parent, value) =>
                created = CreateSystemLeave(graph, parent, value));
        var sut = new LeaveValueLookupVM(
            valueType, service, new DefaultRelayCommandFactory());

        sut.SystemValue = "3";

        sut.Status.Should().Be(LeaveAttributeMatchStatus.NotFound);
        sut.CreateCommand.CanExecute(null).Should().BeTrue();
        sut.CreateCommand.Execute(null);
        sut.CreatedLeave.Should().BeSameAs(created);
        sut.ResolvedMatch.Should().BeSameAs(created);
        sut.CreateCommand.CanExecute(null).Should().BeFalse();

        sut.SystemValue = "дубль";
        sut.CreateCommand.CanExecute(null).Should().BeFalse();

        var boolType = new SystemBaseTreeNodeModel(
            graph.Owner.Parent,
            graph.Tree,
            SystemBaseType.BOOL,
            graph.Notifications,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var boolLookup = new LeaveValueLookupVM(
            boolType, service, new DefaultRelayCommandFactory()) { SystemValue = "новое" };
        boolLookup.Status.Should().Be(LeaveAttributeMatchStatus.NotFound);
        boolLookup.CreateCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void LeaveValueLookup_DraftsRecalculateAndCreateExplicitly()
    {
        var graph = CreateGraph();
        var draft = LeaveAttributeValueDraft.Scalar(Guid.NewGuid(), graph.First.Uuid);
        IReadOnlyList<LeaveAttributeValueDraft>? createdFrom = null;
        TreeLeaveModel? created = null;
        var service = new StubLeaveAttributeValueService(
            (_, _) => new(LeaveAttributeMatchStatus.Invalid, []),
            findDrafts: (_, _) => created == null
                ? new(LeaveAttributeMatchStatus.NotFound, [])
                : new(LeaveAttributeMatchStatus.Resolved, [created]),
            createLeave: (parent, drafts) =>
            {
                createdFrom = drafts.ToArray();
                return created = CreateLeave(parent, graph.Tree, graph.Notifications, "создан");
            });
        var sut = new LeaveValueLookupVM(
            graph.Attribute.ValueType!, service, new DefaultRelayCommandFactory());

        sut.Status.Should().Be(LeaveAttributeMatchStatus.Invalid);
        sut.SetAttributeValues([draft]);

        sut.Status.Should().Be(LeaveAttributeMatchStatus.NotFound);
        sut.CreatedLeave.Should().BeNull();
        sut.CreateCommand.Execute(null);
        createdFrom.Should().Equal(draft);
        sut.CreatedLeave.Should().BeSameAs(created);
        sut.Status.Should().Be(LeaveAttributeMatchStatus.Resolved);
    }

    [Fact]
    public void Selection_ImmediatelyUpdatesCollectionWithoutDuplicates()
    {
        var graph = CreateGraph();
        var sut = new AttributeValuesCollectionVM(graph.Attribute);
        var row = sut.Rows.Single(x => x.SourceUuid == graph.Second.Uuid);

        row["IsSelected"] = true;
        row["IsSelected"] = true;

        graph.Attribute.Values.Should().Equal(graph.Second);
        row["IsSelected"].Should().Be(true);

        row["IsSelected"] = false;

        graph.Attribute.Values.Should().BeEmpty();
        row["IsSelected"].Should().Be(false);
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
        sut.Rows.Single(x => x.SourceUuid == graph.Second.Uuid)
            .CellEnabledStates["IsSelected"].Should().BeFalse();
        sut.Rows.Single(x => x.SourceUuid == graph.Second.Uuid)
            .CellToolTips["IsSelected"].Should().Contain("запечатана");
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

    private static SystemBaseTreeLeaveModel CreateSystemLeave(
        TestGraph graph,
        SystemBaseTreeNodeModel parent,
        string value)
    {
        var result = new SystemBaseTreeLeaveModel(
            Guid.NewGuid(), parent, graph.Tree, parent.SystemBaseType,
            graph.Notifications, new EmptyPropertiesPolicy<TreeLeaveModel>());
        result.StringValue = value;
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

    private sealed class StubLeaveAttributeValueService(
        Func<SystemBaseTreeNodeModel, string?, LeaveAttributeMatchResult> findSystemValue,
        Func<IEnumerable<LeaveAttributeValueDraft>, IEnumerable<TreeLeaveModel>, LeaveAttributeMatchResult>? findDrafts = null,
        Func<SystemBaseTreeNodeModel, string, SystemBaseTreeLeaveModel>? createSystemValue = null,
        Func<TreeNodeModel, IEnumerable<LeaveAttributeValueDraft>, TreeLeaveModel>? createLeave = null)
        : ILeaveAttributeValueService
    {
        public LeaveAttributeMatchResult FindSystemValue(
            SystemBaseTreeNodeModel valueType,
            string? expectedValue) => findSystemValue(valueType, expectedValue);

        public LeaveAttributeMatchResult FindMatches(
            IEnumerable<ElementAttributeModel> expectedAttributes,
            IEnumerable<TreeLeaveModel> candidates) => throw new NotSupportedException();

        public LeaveAttributeMatchResult FindMatches(
            IEnumerable<LeaveAttributeValueDraft> expectedValues,
            IEnumerable<TreeLeaveModel> candidates) =>
            findDrafts?.Invoke(expectedValues, candidates) ?? throw new NotSupportedException();

        public LeaveAttributeFillResult FillFromLeave(
            Philadelphus.Core.Domain.Interfaces.IAttributeOwnerModel targetOwner,
            TreeLeaveModel sourceLeave,
            IEnumerable<Guid> declaringUuids) => throw new NotSupportedException();

        public int CountFillChanges(
            Philadelphus.Core.Domain.Interfaces.IAttributeOwnerModel targetOwner,
            TreeLeaveModel sourceLeave,
            IEnumerable<Guid> declaringUuids) => throw new NotSupportedException();

        public TreeLeaveModel CreateLeave(
            TreeNodeModel parentNode,
            IEnumerable<ElementAttributeModel> sourceAttributes) => throw new NotSupportedException();

        public TreeLeaveModel CreateLeave(
            TreeNodeModel parentNode,
            IEnumerable<LeaveAttributeValueDraft> sourceValues) =>
            createLeave?.Invoke(parentNode, sourceValues) ?? throw new NotSupportedException();

        public SystemBaseTreeLeaveModel CreateSystemValue(
            SystemBaseTreeNodeModel valueType,
            string value) =>
            createSystemValue?.Invoke(valueType, value) ?? throw new NotSupportedException();
    }
}
