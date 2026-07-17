using FluentAssertions;
using Moq;

using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.LeaveAttributeValues.Services;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Services;

/// <summary>
/// Проверяет вычисление и восстановление runtime-связей полиморфных листов.
/// </summary>
public class LeavePolymorphismServiceTests
{
    [Fact]
    public void ResolveParent_ResolvesUniqueActiveCandidate()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        graph.SecondParent.AuditInfo.IsDeleted = true;

        var result = CreateService().ResolveParent(graph.FirstChild);

        result.Status.Should().Be(LeavePolymorphismStatus.Resolved);
        result.ParentLeave.Should().BeSameAs(graph.FirstParent);
        result.Candidates.Should().Equal(graph.FirstParent);
        graph.FirstParent.PolymorphicChildLeaves.Should().Equal(graph.FirstChild);
    }

    [Fact]
    public void ResolveParent_ClearsPreviousLinkWhenResultBecomesAmbiguous()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService();
        service.ResolveParent(graph.FirstChild);
        SetValue(graph.SecondParent, graph, graph.FirstValue);

        var result = service.ResolveParent(graph.FirstChild);

        result.Status.Should().Be(LeavePolymorphismStatus.Ambiguous);
        result.Candidates.Should().Equal(graph.FirstParent, graph.SecondParent);
        graph.FirstChild.PolymorphicParentLeave.Should().BeNull();
        graph.FirstParent.PolymorphicChildLeaves.Should().BeEmpty();
    }

    [Fact]
    public void ResolveParent_DistinguishesNotFoundAndInvalid()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.SecondValue);
        var service = CreateService();

        service.ResolveParent(graph.FirstChild).Status
            .Should().Be(LeavePolymorphismStatus.NotFound);
        GetAttribute(graph.FirstChild, graph).ValueFormulaErrorCode = "FORMULA_ERROR";
        service.ResolveParent(graph.FirstChild).Status
            .Should().Be(LeavePolymorphismStatus.Invalid);
    }

    [Fact]
    public void RefreshLinks_ResolvesEveryPassedLeave()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        SetValue(graph.SecondChild, graph, graph.SecondValue);

        var results = CreateService().RefreshLinks([graph.FirstChild, graph.SecondChild]);

        results.Select(x => x.Status).Should().OnlyContain(x =>
            x == LeavePolymorphismStatus.Resolved);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(graph.FirstParent);
        graph.SecondChild.PolymorphicParentLeave.Should().BeSameAs(graph.SecondParent);
    }

    private static LeavePolymorphismService CreateService() =>
        new(new LeaveAttributeValueService(Mock.Of<IPhiladelphusRepositoryService>()));

    private static TestGraph CreateGraph()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parentNode = CreateNode(root, tree, notifications);
        var childNode = CreateNode(parentNode, tree, notifications);
        var valueType = CreateNode(root, tree, notifications);
        var declarationUuid = Guid.NewGuid();
        _ = new ElementAttributeModel(declarationUuid, parentNode, declarationUuid,
            parentNode, tree, notifications, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = valueType
        };

        return new(tree, declarationUuid,
            CreateLeave(parentNode, tree, notifications), CreateLeave(parentNode, tree, notifications),
            CreateLeave(childNode, tree, notifications), CreateLeave(childNode, tree, notifications),
            CreateLeave(valueType, tree, notifications), CreateLeave(valueType, tree, notifications));
    }

    private static TreeNodeModel CreateNode(IParentModel parent, WorkingTreeModel tree,
        FakeNotificationService notifications) =>
        new(Guid.NewGuid(), parent, tree, notifications, new EmptyPropertiesPolicy<TreeNodeModel>());

    private static TreeLeaveModel CreateLeave(TreeNodeModel parent, WorkingTreeModel tree,
        FakeNotificationService notifications) =>
        new(Guid.NewGuid(), parent, tree, notifications, new EmptyPropertiesPolicy<TreeLeaveModel>());

    private static ElementAttributeModel GetAttribute(TreeLeaveModel leave, TestGraph graph) =>
        leave.Attributes.Single(x => x.DeclaringUuid == graph.DeclarationUuid);

    private static void SetValue(TreeLeaveModel leave, TestGraph graph, TreeLeaveModel value) =>
        GetAttribute(leave, graph).Value = value;

    private sealed record TestGraph(WorkingTreeModel Tree, Guid DeclarationUuid,
        TreeLeaveModel FirstParent, TreeLeaveModel SecondParent,
        TreeLeaveModel FirstChild, TreeLeaveModel SecondChild,
        TreeLeaveModel FirstValue, TreeLeaveModel SecondValue);
}
