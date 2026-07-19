using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.ImportExport.Mapping;
using Philadelphus.Core.Domain.LeaveAttributeValues.Services;
using Philadelphus.Core.Domain.LeavePolymorphism.Services;
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
    public void FillFromParent_PreviewsAndFillsOnlyDirectParentAttributes()
    {
        var graph = CreateGraph();
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService(graph);

        var changedCount = service.CountFillFromParentChanges(
            graph.FirstChild,
            graph.SecondParent);

        changedCount.Should().Be(1);
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.FirstValue);

        var result = service.FillFromParent(graph.FirstChild, graph.SecondParent);

        result.ChangedAttributes.Should().ContainSingle()
            .Which.Should().BeSameAs(GetAttribute(graph.FirstChild, graph));
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.SecondValue);
        GetAttribute(graph.FirstChild, graph).ValueFormula
            .Should().Be($"=[{graph.SecondValue.Uuid}]");
    }

    [Fact]
    public void FillFromParent_FillsAndResolvesNodeRuntimeLink()
    {
        var graph = CreateGraph();
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        GetAttribute(graph.ChildNode, graph).Value = graph.FirstValue;
        var service = CreateService(graph);

        service.CountFillFromParentChanges(graph.ChildNode, graph.SecondParent)
            .Should().Be(1);
        service.FillFromParent(graph.ChildNode, graph.SecondParent);
        var status = service.ResolveParent(graph.ChildNode);

        status.Should().Be(LeavePolymorphismStatus.Resolved);
        graph.ChildNode.PolymorphicParentLeave.Should().BeSameAs(graph.SecondParent);
        GetAttribute(graph.ChildNode, graph).Value.Should().BeSameAs(graph.SecondValue);
    }

    [Fact]
    public void CreateParentChain_CreatesAndLinksNodeParents()
    {
        var graph = CreateGraph();
        GetAttribute(graph.ChildNode, graph).Value = graph.FirstValue;

        var created = CreateService(graph).CreateParentChain(graph.ChildNode);

        created.Should().HaveCount(2);
        created[0].ParentNode.Should().BeSameAs(graph.ParentNode);
        graph.ChildNode.PolymorphicParentLeave.Should().BeSameAs(created[0]);
        created[0].PolymorphicParentLeave.Should().BeSameAs(created[1]);
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

    [Fact]
    public void ApplyPropagation_UpdatesResolvedDescendantsFromTopToBottom()
    {
        var graph = CreateGraph();
        var ancestor = CreateLeave(
            graph.GrandParentNode, graph.Tree, graph.Notifications);
        SetValue(ancestor, graph, graph.FirstValue);
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstParent);
        service.ResolveParent(graph.FirstChild);
        SetValue(ancestor, graph, graph.SecondValue);
        var plan = service.BuildPropagationPlan(ancestor);

        service.ApplyPropagation(plan);

        GetAttribute(graph.FirstParent, graph).Value.Should().BeSameAs(graph.SecondValue);
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.SecondValue);
        graph.FirstParent.PolymorphicParentLeave.Should().BeSameAs(ancestor);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(graph.FirstParent);
        service.IsPropagationInProgress.Should().BeFalse();
    }

    [Fact]
    public void ApplyPropagation_RejectsReentryAndResetsGuard()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var planningService = CreateService(graph);
        planningService.ResolveParent(graph.FirstChild);
        SetValue(graph.FirstParent, graph, graph.SecondValue);
        var plan = planningService.BuildPropagationPlan(graph.FirstParent);
        var attributeValueService = new Mock<ILeaveAttributeValueService>();
        LeavePolymorphismService? service = null;
        attributeValueService
            .Setup(x => x.FillFromLeave(
                It.IsAny<TreeLeaveModel>(),
                It.IsAny<TreeLeaveModel>(),
                It.IsAny<IEnumerable<Guid>>()))
            .Callback(() => service!.ApplyPropagation(plan))
            .Returns(new LeaveAttributeFillResult([]));
        service = new(attributeValueService.Object);

        var action = () => service.ApplyPropagation(plan);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже выполняется*");
        service.IsPropagationInProgress.Should().BeFalse();
        attributeValueService.Verify(x => x.FillFromLeave(
            graph.FirstChild,
            graph.FirstParent,
            It.IsAny<IEnumerable<Guid>>()), Times.Once);
    }

    [Fact]
    public void ApplyPropagation_RejectsPlanWhenRuntimeLinkHasChanged()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstChild);
        SetValue(graph.FirstParent, graph, graph.SecondValue);
        var plan = service.BuildPropagationPlan(graph.FirstParent);
        service.ResolveParent(graph.FirstChild);

        var action = () => service.ApplyPropagation(plan);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*изменилась после расчёта*");
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.FirstValue);
        service.IsPropagationInProgress.Should().BeFalse();
    }

    [Fact]
    public void PreserveChildren_ReusesExistingReplacement()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstChild);
        SetValue(graph.FirstParent, graph, graph.SecondValue);
        SetValue(graph.SecondParent, graph, graph.FirstValue);
        var plan = service.BuildPropagationPlan(graph.FirstParent);

        var result = service.PreserveChildrenAndResolveReplacement(plan);

        result.CreatedLeaves.Should().BeEmpty();
        result.Resolutions.Should().ContainSingle()
            .Which.Status.Should().Be(LeavePolymorphismStatus.Resolved);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(graph.SecondParent);
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.FirstValue);
    }

    [Fact]
    public void PreserveChildren_CreatesOneReplacementChainForEqualSignatures()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        SetValue(graph.SecondChild, graph, graph.FirstValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstChild);
        service.ResolveParent(graph.SecondChild);
        SetValue(graph.FirstParent, graph, graph.SecondValue);
        var plan = service.BuildPropagationPlan(graph.FirstParent);

        var result = service.PreserveChildrenAndResolveReplacement(plan);

        var replacement = result.CreatedLeaves
            .Should().ContainSingle(x => ReferenceEquals(x.ParentNode, graph.ParentNode))
            .Which;
        result.CreatedLeaves.Should().HaveCount(2);
        result.Resolutions.Should().HaveCount(2)
            .And.OnlyContain(x => x.Status == LeavePolymorphismStatus.Resolved);
        graph.FirstChild.PolymorphicParentLeave.Should().BeSameAs(replacement);
        graph.SecondChild.PolymorphicParentLeave.Should().BeSameAs(replacement);
        GetAttribute(replacement, graph).Value.Should().BeSameAs(graph.FirstValue);
    }

    [Fact]
    public void PreserveChildren_LeavesAmbiguityWithoutCreatingDuplicate()
    {
        var graph = CreateGraph();
        var duplicateParent = CreateLeave(
            graph.ParentNode, graph.Tree, graph.Notifications);
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.SecondParent, graph, graph.SecondValue);
        SetValue(duplicateParent, graph, graph.SecondValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);
        var service = CreateService(graph);
        service.ResolveParent(graph.FirstChild);
        SetValue(graph.FirstParent, graph, graph.SecondValue);
        SetValue(graph.SecondParent, graph, graph.FirstValue);
        SetValue(duplicateParent, graph, graph.FirstValue);
        var plan = service.BuildPropagationPlan(graph.FirstParent);

        var result = service.PreserveChildrenAndResolveReplacement(plan);

        result.CreatedLeaves.Should().BeEmpty();
        result.Resolutions.Should().ContainSingle()
            .Which.Status.Should().Be(LeavePolymorphismStatus.Ambiguous);
        graph.FirstChild.PolymorphicParentLeave.Should().BeNull();
        GetAttribute(graph.FirstChild, graph).Value.Should().BeSameAs(graph.FirstValue);
    }

    [Fact]
    public void RuntimeAttribute_IsCachedAndSynchronizesParentDefinition()
    {
        var graph = CreateGraph();

        var runtimeAttributes = graph.ChildNode.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .ToList();
        runtimeAttributes.Should().HaveCount(1);
        var attribute = runtimeAttributes.Single();
        var parentAttribute = graph.ParentNode.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which;
        graph.ParentNode.Name = "Переименованный родитель";

        attribute.IsRuntime.Should().BeTrue();
        attribute.IsPolymorphic.Should().BeTrue();
        attribute.Name.Should().Be(graph.ParentNode.Name);
        attribute.ValueType.Should().BeSameAs(graph.ParentNode);
        attribute.IsCollectionValue.Should().BeFalse();
        attribute.Visibility.Should().Be(VisibilityScope.Protected);
        attribute.DeclaringUuid.Should().Be(parentAttribute.DeclaringUuid);
        attribute.LocalUuid.Should().NotBe(parentAttribute.LocalUuid);
        graph.ChildNode.Attributes.OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which.Should().BeSameAs(attribute);
    }

    [Fact]
    public void RuntimeAttribute_MaterializesForDirectLeaveAndReflectsResolution()
    {
        var graph = CreateGraph();
        SetValue(graph.FirstParent, graph, graph.FirstValue);
        SetValue(graph.FirstChild, graph, graph.FirstValue);

        var resolution = CreateService(graph).ResolveParent(graph.FirstChild);

        var attribute = graph.FirstChild.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which;
        attribute.DeclaringUuid.Should().Be(graph.ParentNode.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which.DeclaringUuid);
        attribute.Status.Should().Be(LeavePolymorphismStatus.Resolved);
        attribute.Candidates.Should().Equal(graph.FirstParent);
        attribute.Value.Should().BeSameAs(graph.FirstParent);
        resolution.ParentLeave.Should().BeSameAs(attribute.Value);
    }

    [Fact]
    public void RuntimeAttribute_IsNotCreatedForRootLevelOrSystemNode()
    {
        var graph = CreateGraph();
        var systemNode = new SystemBaseTreeNodeModel(
            graph.ParentNode,
            graph.Tree,
            SystemBaseType.STRING,
            graph.Notifications,
            new EmptyPropertiesPolicy<TreeNodeModel>());

        graph.GrandParentNode.Attributes
            .Should().NotContain(x => x is LeavePolymorphismAttributeModel);
        systemNode.Attributes
            .Should().NotContain(x => x is LeavePolymorphismAttributeModel);
    }

    [Fact]
    public void RuntimeAttribute_DoesNotParticipateInPersistenceOrDirtyState()
    {
        var graph = CreateGraph();
        var attribute = graph.ChildNode.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which;
        var repositoryService = new PhiladelphusRepositoryService(
            Mock.Of<IMapper>(), Mock.Of<ILogger>(), graph.Notifications);

        var savedCount = repositoryService.SaveContentChanges([attribute]);

        savedCount.Should().Be(0);
        attribute.State.Should().Be(State.SavedOrLoaded);
        graph.Tree.ContentAttributes.Should().NotContain(x => x.IsRuntime);
    }

    [Fact]
    public void RuntimeAttribute_IsExcludedFromImportExportDto()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>()) { Name = "Root" };
        var parent = CreateNode(root, tree, notifications);
        parent.Name = "Parent";
        var child = CreateNode(parent, tree, notifications);
        child.Name = "Child";
        var runtimeAttribute = child.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Should().ContainSingle().Which;
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<ImportExportDtoMappingProfile>(),
            NullLoggerFactory.Instance);

        var dto = configuration.CreateMapper().Map<TreeNodeExportDTO>(child);

        dto.Attributes.Should().NotContain(x => x.Name == runtimeAttribute.Name);
    }

    [Fact]
    public void RuntimeAttribute_IsExcludedFromLeaveStringValueJson()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parent = CreateNode(root, tree, notifications);
        parent.Name = "Parent";
        var child = CreateNode(parent, tree, notifications);
        var leave = CreateLeave(child, tree, notifications);

        leave.StringValue.Should().Be("{}");
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

    /// <summary>
    /// Возвращает материализованную копию тестового объявления у узла.
    /// </summary>
    private static ElementAttributeModel GetAttribute(
        TreeNodeModel node,
        LeavePolymorphismTestGraph graph) =>
        node.Attributes.Single(x => x.DeclaringUuid == graph.DeclarationUuid);

    private static void SetValue(
        TreeLeaveModel leave,
        LeavePolymorphismTestGraph graph,
        TreeLeaveModel value) =>
        GetAttribute(leave, graph).Value = value;
}
