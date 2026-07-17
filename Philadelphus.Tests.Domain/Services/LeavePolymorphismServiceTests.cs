using AutoMapper;
using FluentAssertions;
using Moq;

using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
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
using Serilog;

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

        var result = CreateService(graph).ResolveParent(graph.FirstChild);

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
        var service = CreateService(graph);
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
        var service = CreateService(graph);

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

        var results = CreateService(graph).RefreshLinks([graph.FirstChild, graph.SecondChild]);

        results.Select(x => x.Status).Should().OnlyContain(x =>
            x == LeavePolymorphismStatus.Resolved);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(graph.FirstParent);
        graph.SecondChild.PolymorphicParentLeave.Should().BeSameAs(graph.SecondParent);
    }

    [Fact]
    public void CreateParentChain_CreatesAndLinksEveryMissingLevel()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstChild, graph, graph.FirstValue);

        var created = CreateService(graph).CreateParentChain(graph.FirstChild);

        created.Should().HaveCount(2);
        created[0].ParentNode.Should().BeSameAs(graph.ParentNode);
        created[1].ParentNode.Should().BeSameAs(graph.GrandParentNode);
        created.Should().OnlyContain(x => x.State == State.Initialized);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(created[0]);
        created[0].PolymorphicParentLeave.Should().BeSameAs(created[1]);
        GetAttribute(created[0], graph).Value.Should().BeSameAs(graph.FirstValue);
        GetAttribute(created[1], graph).Value.Should().BeSameAs(graph.FirstValue);
    }

    [Fact]
    public void CreateParentChain_ReusesUniqueExistingAncestor()
    {
        var graph = CreateGraph();
        var existingAncestor = CreateLeave(
            graph.GrandParentNode, graph.Tree, graph.Notifications);
        SetValue(existingAncestor, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);

        var created = CreateService(graph).CreateParentChain(graph.FirstChild);

        created.Should().ContainSingle();
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(created.Single());
        created.Single().PolymorphicParentLeave.Should().BeSameAs(existingAncestor);
    }

    [Fact]
    public void CreateParentChain_DoesNotCreatePartialChainWhenUpperLevelIsAmbiguous()
    {
        var graph = CreateGraph();
        var firstAncestor = CreateLeave(
            graph.GrandParentNode, graph.Tree, graph.Notifications);
        var secondAncestor = CreateLeave(
            graph.GrandParentNode, graph.Tree, graph.Notifications);
        SetValue(firstAncestor, graph, graph.FirstValue);
        SetValue(secondAncestor, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var childLeavesBefore = graph.ParentNode.ChildLeaves.ToList();

        var action = () => CreateService(graph).CreateParentChain(graph.FirstChild);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Ambiguous*");
        graph.ParentNode.ChildLeaves.Should().Equal(childLeavesBefore);
        graph.FirstChild.PolymorphicParentLeave.Should().BeNull();
    }

    [Fact]
    public void CreateParentChain_DoesNotCreateLeavesForInvalidValues()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        GetAttribute(graph.FirstChild, graph).ValueFormulaErrorCode = "FORMULA_ERROR";
        var childLeavesBefore = graph.ParentNode.ChildLeaves.ToList();

        var action = () => CreateService(graph).CreateParentChain(graph.FirstChild);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid*");
        graph.ParentNode.ChildLeaves.Should().Equal(childLeavesBefore);
    }

    [Fact]
    public void BuildPropagationPlan_CalculatesTransitiveChangesWithoutMutation()
    {
        var graph = CreateGraph();
        var ancestor = CreateLeave(
            graph.GrandParentNode, graph.Tree, graph.Notifications);
        SetValue(ancestor, graph, graph.FirstValue);
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        SetValue(graph.SecondChild, graph, graph.SecondValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstParent);
        service.ResolveParent(graph.FirstChild);
        SetValue(ancestor, graph, graph.SecondValue);

        var plan = service.BuildPropagationPlan(ancestor);

        plan.ChangedParentLeave.Should().BeSameAs(ancestor);
        plan.AffectedLeaveCount.Should().Be(2);
        plan.ChangedAttributeCount.Should().Be(2);
        plan.Items.Select(x => x.SourceLeave)
            .Should().Equal(ancestor, graph.FirstParent);
        plan.Items.Select(x => x.TargetLeave)
            .Should().Equal(graph.FirstParent, graph.FirstChild);
        plan.Items.Should().OnlyContain(x => x.ChangedAttributeCount == 1);
        GetAttribute(graph.FirstParent, graph).Value.Should().BeSameAs(graph.FirstValue);
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.FirstValue);
        ancestor.PolymorphicChildLeaves.Should().Equal(graph.FirstParent);
        graph.FirstParent.PolymorphicChildLeaves.Should().Equal(graph.FirstChild);
    }

    private static LeavePolymorphismService CreateService(LeavePolymorphismTestGraph graph)
    {
        var repositoryService = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(), Mock.Of<ILogger>(), graph.Notifications);
        return new(new LeaveAttributeValueService(repositoryService));
    }

    private static LeavePolymorphismTestGraph CreateGraph()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var grandParentNode = CreateNode(root, tree, notifications);
        var parentNode = CreateNode(grandParentNode, tree, notifications);
        var childNode = CreateNode(parentNode, tree, notifications);
        var valueType = CreateNode(root, tree, notifications);
        var declarationUuid = Guid.NewGuid();
        _ = new ElementAttributeModel(declarationUuid, grandParentNode, declarationUuid,
            grandParentNode, tree, notifications, new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = valueType
        };

        return new(tree, notifications, grandParentNode, parentNode, childNode, declarationUuid,
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

    private static ElementAttributeModel GetAttribute(
        TreeLeaveModel leave,
        LeavePolymorphismTestGraph graph) =>
        leave.Attributes.Single(x => x.DeclaringUuid == graph.DeclarationUuid);

    private static void SetValue(
        TreeLeaveModel leave,
        LeavePolymorphismTestGraph graph,
        TreeLeaveModel value) =>
        GetAttribute(leave, graph).Value = value;
}
