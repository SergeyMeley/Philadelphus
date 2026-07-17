using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Описывает обновление одного разрешённого полиморфного наследника.
/// </summary>
public sealed class LeavePolymorphismPropagationItem
{
    /// <summary>
    /// Создаёт защищённый снимок одного обновления.
    /// </summary>
    /// <param name="sourceLeave">Родительский лист.</param>
    /// <param name="targetLeave">Обновляемый наследник.</param>
    /// <param name="declaringUuids">Объявления переносимых атрибутов.</param>
    /// <param name="changedAttributeCount">Количество фактических изменений.</param>
    internal LeavePolymorphismPropagationItem(
        TreeLeaveModel sourceLeave,
        TreeLeaveModel targetLeave,
        IReadOnlyList<Guid> declaringUuids,
        int changedAttributeCount)
    {
        ArgumentNullException.ThrowIfNull(sourceLeave);
        ArgumentNullException.ThrowIfNull(targetLeave);
        ArgumentNullException.ThrowIfNull(declaringUuids);

        SourceLeave = sourceLeave;
        TargetLeave = targetLeave;
        DeclaringUuids = declaringUuids.ToList().AsReadOnly();
        ChangedAttributeCount = changedAttributeCount;
    }

    /// <summary>
    /// Родительский лист, из которого переносятся значения.
    /// </summary>
    public TreeLeaveModel SourceLeave { get; }

    /// <summary>
    /// Разрешённый наследник, значения которого требуется обновить.
    /// </summary>
    public TreeLeaveModel TargetLeave { get; }

    /// <summary>
    /// Объявления атрибутов, принадлежащие родительскому уровню.
    /// </summary>
    public IReadOnlyList<Guid> DeclaringUuids { get; }

    /// <summary>
    /// Количество атрибутов, фактически изменяемых этим обновлением.
    /// </summary>
    public int ChangedAttributeCount { get; }
}
