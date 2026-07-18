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

    /// <inheritdoc />
    public LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave)
    {
        ResolveCount++;
        return new(childLeave, LeavePolymorphismStatus.NotFound, null, []);
    }

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
        return new([], []);
    }

    /// <inheritdoc />
    public IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(
        IEnumerable<TreeLeaveModel> leaves) => [];
}
