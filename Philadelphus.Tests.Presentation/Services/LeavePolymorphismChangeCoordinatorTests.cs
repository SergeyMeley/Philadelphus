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
    [Fact]
    public async Task HandleChangedLeaveAsync_WithoutAffectedLeaves_OnlyResolvesChangedLeave()
    {
        var (parentLeave, _) = CreateLeaves();
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, []);
        var polymorphism = new FakeLeavePolymorphismService(plan);
        var confirmation = new FakeLeavePolymorphismConfirmationService(true);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        await coordinator.HandleChangedLeaveAsync(parentLeave);

        confirmation.PropagationCallCount.Should().Be(0);
        polymorphism.ApplyCount.Should().Be(0);
        polymorphism.PreserveCount.Should().Be(0);
        polymorphism.ResolveCount.Should().Be(1);
    }

    [Theory]
    [InlineData(true, 1, 0)]
    [InlineData(false, 0, 1)]
    public async Task HandleChangedLeaveAsync_WithAffectedLeaves_AppliesSelectedBranch(
        bool confirmed,
        int expectedApplyCount,
        int expectedPreserveCount)
    {
        var (parentLeave, childLeave) = CreateLeaves();
        var item = new LeavePolymorphismPropagationItem(
            parentLeave,
            childLeave,
            [],
            changedAttributeCount: 2);
        var plan = new LeavePolymorphismPropagationPlan(parentLeave, [item]);
        var polymorphism = new FakeLeavePolymorphismService(plan);
        var confirmation = new FakeLeavePolymorphismConfirmationService(confirmed);
        var coordinator = new LeavePolymorphismChangeCoordinator(polymorphism, confirmation);

        await coordinator.HandleChangedLeaveAsync(parentLeave);

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
}
