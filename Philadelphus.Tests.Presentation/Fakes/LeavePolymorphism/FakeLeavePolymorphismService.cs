using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.Interfaces;

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

    /// <summary>Число изменений, возвращаемое предварительным расчётом заполнения.</summary>
    internal int ManualFillChangedAttributeCount { get; init; }

    /// <summary>Количество применений ручного заполнения.</summary>
    internal int ManualFillCount { get; private set; }

    /// <summary>Количество запросов создания цепочки.</summary>
    internal int CreateParentChainCount { get; private set; }

    /// <summary>Количество массовых обновлений runtime-связей.</summary>
    internal int RefreshLinksCount { get; private set; }

    /// <summary>Последний получатель ручного заполнения.</summary>
    internal IAttributeOwnerModel? LastManualFillOwner { get; private set; }

    /// <summary>Последний узел, для которого пересчитывалась runtime-связь.</summary>
    internal TreeNodeModel? LastResolvedNode { get; private set; }

    /// <summary>Последний набор листов, переданный для массового пересчёта runtime-связей.</summary>
    internal IReadOnlyList<TreeLeaveModel> LastRefreshedLeaves { get; private set; } = [];

    /// <summary>Листы, которые fake-сервис возвращает из ветки сохранения.</summary>
    internal IReadOnlyList<TreeLeaveModel> PreservedCreatedLeaves { get; init; } = [];

    /// <inheritdoc />
    public LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave)
    {
        ResolveCount++;
        return new(childLeave, LeavePolymorphismStatus.NotFound, null, []);
    }

    /// <inheritdoc />
    public LeavePolymorphismStatus ResolveParent(TreeNodeModel childNode)
    {
        ResolveCount++;
        LastResolvedNode = childNode;
        return LeavePolymorphismStatus.NotFound;
    }

    /// <inheritdoc />
    public int CountFillFromParentChanges(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave) => ManualFillChangedAttributeCount;

    /// <inheritdoc />
    public LeaveAttributeFillResult FillFromParent(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave)
    {
        ManualFillCount++;
        LastManualFillOwner = childOwner;
        return new([]);
    }

    /// <inheritdoc />
    public IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeLeaveModel childLeave)
    {
        CreateParentChainCount++;
        return CreatedParentChainLeaves;
    }

    /// <inheritdoc />
    public IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeNodeModel childNode)
    {
        CreateParentChainCount++;
        return CreatedParentChainLeaves;
    }

    /// <summary>Листы, возвращаемые операцией создания цепочки.</summary>
    internal IReadOnlyList<TreeLeaveModel> CreatedParentChainLeaves { get; init; } = [];

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
        IEnumerable<TreeLeaveModel> leaves)
    {
        RefreshLinksCount++;
        LastRefreshedLeaves = leaves.ToList();
        return [];
    }

    /// <inheritdoc />
    public void RefreshLinks(IEnumerable<TreeNodeModel> nodes) =>
        RefreshLinksCount++;
}
