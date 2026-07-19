using FluentAssertions;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Presentation.Services;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Philadelphus.Tests.Presentation.Fakes.LeavePolymorphism;

namespace Philadelphus.Tests.Presentation.Services;

/// <summary>
/// Проверяет выбор ветки интерактивного каскдного обновления.
/// </summary>
public sealed class LeavePolymorphismChangeCoordinatorTests
{
    [Theory]
    [InlineData(true, true, 1, 1)]
    [InlineData(false, false, 0, 0)]
    public async Task FillFromParentAsync_AppliesOnlyConfirmedFill(
        bool confirmed,
        bool expectedApplied,
        int expectedFillCount,
        int expectedResolveCount)
    {
        var (parentLeave, childLeave) = CreateLeaves();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan)
        {
            ManualFillChangedAttributeCount = 2
        };
        var confirmation = new FakeLeavePolymorphismConfirmationService(
            propagationConfirmed: true,
            manualFillConfirmed: confirmed);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = await coordinator.FillFromParentAsync(childLeave, parentLeave);

        result.Applied.Should().Be(expectedApplied);
        confirmation.ManualFillCallCount.Should().Be(1);
        confirmation.LastManualFillChangedAttributeCount.Should().Be(2);
        polymorphism.ManualFillCount.Should().Be(expectedFillCount);
        polymorphism.ResolveCount.Should().Be(expectedResolveCount);
    }

    [Fact]
    public async Task FillFromParentAsync_WithoutChanges_SkipsConfirmation()
    {
        var (parentLeave, childLeave) = CreateLeaves();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan);
        var confirmation = new FakeLeavePolymorphismConfirmationService(
            propagationConfirmed: true,
            manualFillConfirmed: false);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = await coordinator.FillFromParentAsync(childLeave, parentLeave);

        result.Applied.Should().BeTrue();
        result.ChangedAttributes.Should().BeEmpty();
        confirmation.ManualFillCallCount.Should().Be(0);
        polymorphism.ManualFillCount.Should().Be(1);
        polymorphism.ResolveCount.Should().Be(1);
    }

    /// <summary>
    /// Подтверждённый выбор родителя для узла должен заполнить сам узел и пересчитать его runtime-связь.
    /// </summary>
    [Fact]
    public async Task FillFromParentAsync_NodeRecipient_FillsAndResolvesNode()
    {
        var (parentLeave, childNode) = CreateNodeRecipient();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan)
        {
            ManualFillChangedAttributeCount = 1
        };
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = await coordinator.FillFromParentAsync(childNode, parentLeave);

        result.Applied.Should().BeTrue();
        polymorphism.LastManualFillOwner.Should().BeSameAs(childNode);
        polymorphism.LastResolvedNode.Should().BeSameAs(childNode);
        confirmation.ManualFillCallCount.Should().Be(1);
    }

    [Fact]
    public void CreateParentChain_RefreshesCreatedRuntimeLinksWithoutConfirmation()
    {
        var (parentLeave, childLeave) = CreateLeaves();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan)
        {
            CreatedParentChainLeaves = [parentLeave]
        };
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = coordinator.CreateParentChain(childLeave);

        result.CascadeProcessed.Should().BeFalse();
        result.CreatedLeaves.Should().Equal(parentLeave);
        polymorphism.CreateParentChainCount.Should().Be(1);
        polymorphism.RefreshLinksCount.Should().Be(1);
        confirmation.ManualFillCallCount.Should().Be(0);
        confirmation.PropagationCallCount.Should().Be(0);
    }

    /// <summary>
    /// Создание родителя узла должно разрешить связь узла и восстановить связи созданной цепочки.
    /// </summary>
    [Fact]
    public void CreateParentChain_NodeRecipient_RefreshesChainAndResolvesNode()
    {
        var (parentLeave, childNode) = CreateNodeRecipient();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan)
        {
            CreatedParentChainLeaves = [parentLeave]
        };
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = coordinator.CreateParentChain(childNode);

        result.CreatedLeaves.Should().Equal(parentLeave);
        polymorphism.LastRefreshedLeaves.Should().Equal(parentLeave);
        polymorphism.LastResolvedNode.Should().BeSameAs(childNode);
        confirmation.ManualFillCallCount.Should().Be(0);
        confirmation.PropagationCallCount.Should().Be(0);
    }

    /// <summary>
    /// Изменение обычного атрибута узла должно пересчитать сам узел и его непосредственные листья.
    /// </summary>
    [Fact]
    public void HandleChangedNode_ResolvesNodeAndItsDirectLeaves()
    {
        var (parentLeave, childNode) = CreateNodeRecipient();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan);
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = coordinator.HandleChangedNode(childNode);

        result.CascadeProcessed.Should().BeFalse();
        polymorphism.LastResolvedNode.Should().BeSameAs(childNode);
        polymorphism.LastRefreshedLeaves.Should().Equal(childNode.ChildLeaves);
        confirmation.ManualFillCallCount.Should().Be(0);
        confirmation.PropagationCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleChangedLeaveAsync_WithoutAffectedLeaves_OnlyResolvesChangedLeave()
    {
        var (parentLeave, _) = CreateLeaves();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan);
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = await coordinator.HandleChangedLeaveAsync(parentLeave);

        result.CascadeProcessed.Should().BeFalse();
        result.CreatedLeaves.Should().BeEmpty();
        confirmation.PropagationCallCount.Should().Be(0);
        polymorphism.ApplyCount.Should().Be(0);
        polymorphism.PreserveCount.Should().Be(0);
        polymorphism.ResolveCount.Should().Be(1);
    }

    [Theory]
    [InlineData(true, 1, 0, 0)]
    [InlineData(false, 0, 1, 1)]
    public async Task HandleChangedLeaveAsync_WithAffectedLeaves_AppliesSelectedBranch(
        bool confirmed,
        int expectedApplyCount,
        int expectedPreserveCount,
        int expectedCreatedLeaveCount)
    {
        var (parentLeave, childLeave) = CreateLeaves();
        var item = new LeavePolymorphismPropagationItem(
            parentLeave,
            childLeave,
            [],
            changedAttributeCount: 2);
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, [item]);
        var polymorphism = new FakeLeavePolymorphismService(plan)
        {
            PreservedCreatedLeaves = confirmed ? [] : [childLeave]
        };
        var confirmation = new FakeLeavePolymorphismConfirmationService(confirmed);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        var result = await coordinator.HandleChangedLeaveAsync(parentLeave);

        result.CascadeProcessed.Should().BeTrue();
        result.CreatedLeaves.Should().HaveCount(expectedCreatedLeaveCount);
        confirmation.PropagationCallCount.Should().Be(1);
        polymorphism.ApplyCount.Should().Be(expectedApplyCount);
        polymorphism.PreserveCount.Should().Be(expectedPreserveCount);
        polymorphism.ResolveCount.Should().Be(1);
    }

    /// <summary>
    /// Создаёт два листа одного узла для сборки тестового плана.
    /// </summary>
    private static (TreeLeaveModel ParentLeave, TreeLeaveModel ChildLeave) CreateLeaves()
    {
        var tree = new FakeWorkingTreeModel();
        var notificationService = new FakeNotificationService();
        var root = new TreeRootModel(
            Guid.CreateVersion7(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var node = new TreeNodeModel(
            Guid.CreateVersion7(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());

        return (
            new TreeLeaveModel(
                Guid.CreateVersion7(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>()),
            new TreeLeaveModel(
                Guid.CreateVersion7(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>()));
    }

    /// <summary>
    /// Создаёт родительский лист и дочерний узел с непосредственным листом.
    /// </summary>
    private static (TreeLeaveModel ParentLeave, TreeNodeModel ChildNode) CreateNodeRecipient()
    {
        var tree = new FakeWorkingTreeModel();
        var notificationService = new FakeNotificationService();
        var root = new TreeRootModel(
            Guid.CreateVersion7(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parentNode = new TreeNodeModel(
            Guid.CreateVersion7(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var parentLeave = new TreeLeaveModel(
            Guid.CreateVersion7(),
            parentNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var childNode = new TreeNodeModel(
            Guid.CreateVersion7(),
            parentNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        _ = new TreeLeaveModel(
            Guid.CreateVersion7(),
            childNode,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());

        return (parentLeave, childNode);
    }
}
