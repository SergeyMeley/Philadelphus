using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Tests.Presentation.Fakes.LeavePolymorphism;

/// <summary>
/// Фиксирует выбранную координатором ветку доменной полиморфной операции.
/// </summary>
internal sealed class FakeLeavePolymorphismService : ILeavePolymorphismService
{
    private readonly LeavePolymorphismPropagationPlan _plan;

    /// <summary>
    /// Инициализирует fake-сервис возвращаемым планом.
    /// </summary>
    internal FakeLeavePolymorphismService(LeavePolymorphismPropagationPlan plan) =>
        _plan = plan;

    /// <inheritdoc />
    public bool IsPropagationInProgress => false;

    /// <summary>Количество применений плана.</summary>
    internal int ApplyCount { get; private set; }

    /// <summary>Количество выборов ветки сохранения.</summary>
    internal int PreserveCount { get; private set; }

    /// <summary>Количество пересчётов связи изменённого листа.</summary>
    internal int ResolveCount { get; private set; }

    /// <summary>Листы, которые fake-сервис возвращает из ветки сохранения.</summary>
    internal IReadOnlyList<TreeLeaveModel> PreservedCreatedLeaves { get; init; } = [];

    /// <inheritdoc />
    public LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave)
    {
        ResolveCount++;
        return new(childLeave, LeavePolymorphismStatus.NotFound, null, []);
    }

    /// <inheritdoc />
    public int CountFillFromParentChanges(
        TreeLeaveModel childLeave,
        TreeLeaveModel parentLeave) => 0;

    /// <inheritdoc />
    public LeaveAttributeFillResult FillFromParent(
        TreeLeaveModel childLeave,
        TreeLeaveModel parentLeave) => new([]);

    /// <inheritdoc />
    public IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeLeaveModel childLeave) => [];

    /// <inheritdoc />
    public LeavePolymorphismPropagationPlan BuildPropagationPlan(TreeLeaveModel changedParentLeave) =>
        _plan;

    /// <inheritdoc />
    public void ApplyPropagation(LeavePolymorphismPropagationPlan plan) =>
        ApplyCount++;

    /// <inheritdoc />
    public LeavePolymorphismPreservationResult PreserveChildrenAndResolveReplacement(
        LeavePolymorphismPropagationPlan plan)
    {
        PreserveCount++;
        return new([], PreservedCreatedLeaves);
    }

    /// <inheritdoc />
    public IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(
        IEnumerable<TreeLeaveModel> leaves) => [];
}
