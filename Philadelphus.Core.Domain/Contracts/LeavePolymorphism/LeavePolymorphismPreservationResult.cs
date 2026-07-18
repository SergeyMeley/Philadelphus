using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Результат сохранения прежних значений наследников после отказа от каскадного обновления.
/// </summary>
public sealed class LeavePolymorphismPreservationResult
{
    /// <summary>
    /// Создаёт защищённый снимок результатов операции.
    /// </summary>
    /// <param name="resolutions">Итоговые разрешения непосредственных наследников.</param>
    /// <param name="createdLeaves">Созданные заменяющие листы и их предки.</param>
    internal LeavePolymorphismPreservationResult(
        IReadOnlyList<LeavePolymorphismResolution> resolutions,
        IReadOnlyList<TreeLeaveModel> createdLeaves)
    {
        ArgumentNullException.ThrowIfNull(resolutions);
        ArgumentNullException.ThrowIfNull(createdLeaves);

        Resolutions = resolutions.ToList().AsReadOnly();
        CreatedLeaves = createdLeaves.ToList().AsReadOnly();
    }

    /// <summary>
    /// Итоговые статусы непосредственных наследников изменённого листа.
    /// </summary>
    public IReadOnlyList<LeavePolymorphismResolution> Resolutions { get; }

    /// <summary>
    /// Все созданные листы в порядке от непосредственных замен к верхним предкам.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> CreatedLeaves { get; }
}
