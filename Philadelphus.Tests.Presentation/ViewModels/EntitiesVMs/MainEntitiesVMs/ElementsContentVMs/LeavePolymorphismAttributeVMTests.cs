using FluentAssertions;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

/// <summary>
/// Проверяет временное UI-состояние строки полиморфного родителя.
/// </summary>
public sealed class LeavePolymorphismAttributeVMTests
{
    [Fact]
    public void SelectedCandidate_AcceptsOnlyCurrentResolutionCandidate()
    {
        var (attribute, firstCandidate, secondCandidate, unrelatedLeave) = CreateFixture();
        attribute.SetResolution(
            LeavePolymorphismStatus.Ambiguous,
            [firstCandidate, secondCandidate]);
        var viewModel = new LeavePolymorphismAttributeVM(attribute);

        viewModel.SelectedCandidate = unrelatedLeave;
        viewModel.SelectedCandidate.Should().BeNull();
        viewModel.CanApplyCandidate.Should().BeFalse();

        viewModel.SelectedCandidate = secondCandidate;
        viewModel.SelectedCandidate.Should().BeSameAs(secondCandidate);
        viewModel.CanApplyCandidate.Should().BeTrue();
        viewModel.DisplayText.Should().Be("Неоднозначно");
    }

    [Fact]
    public void NotifyResolutionChanged_ClearsSelectionAndDisplaysResolvedParent()
    {
        var (attribute, firstCandidate, secondCandidate, _) = CreateFixture();
        attribute.SetResolution(
            LeavePolymorphismStatus.Ambiguous,
            [firstCandidate, secondCandidate]);
        var viewModel = new LeavePolymorphismAttributeVM(attribute)
        {
            SelectedCandidate = firstCandidate
        };

        viewModel.RecipientLeave!.SetPolymorphicParentLeave(secondCandidate);
        attribute.SetResolution(LeavePolymorphismStatus.Resolved, [secondCandidate]);
        viewModel.NotifyResolutionChanged();

        viewModel.SelectedCandidate.Should().BeNull();
        viewModel.Status.Should().Be(LeavePolymorphismStatus.Resolved);
        viewModel.DisplayText.Should().Be(secondCandidate.Name);
        viewModel.CanCreateParent.Should().BeFalse();
    }

    /// <summary>
    /// Создаёт лист двухуровневого узла и кандидатов его прямого родителя.
    /// </summary>
    private static (
        LeavePolymorphismAttributeModel Attribute,
        TreeLeaveModel FirstCandidate,
        TreeLeaveModel SecondCandidate,
        TreeLeaveModel UnrelatedLeave) CreateFixture()
    {
        var tree = new FakeWorkingTreeModel();
        var notificationService = new FakeNotificationService();
        var root = new TreeRootModel(
            Guid.CreateVersion7(), tree, notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parentNode = new TreeNodeModel(
            Guid.CreateVersion7(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var childNode = new TreeNodeModel(
            Guid.CreateVersion7(), parentNode, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var firstCandidate = CreateLeave("Parent 1", parentNode, tree, notificationService);
        var secondCandidate = CreateLeave("Parent 2", parentNode, tree, notificationService);
        var childLeave = CreateLeave("Child", childNode, tree, notificationService);
        var unrelatedLeave = CreateLeave("Unrelated", childNode, tree, notificationService);
        var attribute = childLeave.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .Single();

        return (attribute, firstCandidate, secondCandidate, unrelatedLeave);
    }

    /// <summary>
    /// Создаёт именованный лист заданного узла.
    /// </summary>
    private static TreeLeaveModel CreateLeave(
        string name,
        TreeNodeModel parent,
        FakeWorkingTreeModel tree,
        FakeNotificationService notificationService) =>
        new(
            Guid.CreateVersion7(), parent, tree, notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>())
        {
            Name = name
        };
}
