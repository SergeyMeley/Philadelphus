using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Неизменяемый план транзитивного обновления разрешённых наследников.
/// </summary>
public sealed class LeavePolymorphismPropagationPlan
{
    /// <summary>
    /// Создаёт защищённый снимок рассчитанной последовательности обновлений.
    /// </summary>
    /// <param name="changedParentLeave">Исходный изменённый лист.</param>
    /// <param name="items">Рассчитанные обновления сверху вниз.</param>
    internal LeavePolymorphismPropagationPlan(
        TreeLeaveModel changedParentLeave,
        IReadOnlyList<LeavePolymorphismPropagationItem> items)
    {
        ArgumentNullException.ThrowIfNull(changedParentLeave);
        ArgumentNullException.ThrowIfNull(items);

        ChangedParentLeave = changedParentLeave;
        Items = items.ToList().AsReadOnly();
        ChangedAttributeCount = Items.Sum(x => x.ChangedAttributeCount);
    }

    /// <summary>
    /// Исходный изменённый родительский лист.
    /// </summary>
    public TreeLeaveModel ChangedParentLeave { get; }

    /// <summary>
    /// Обновления в порядке от верхних уровней к нижним.
    /// </summary>
    public IReadOnlyList<LeavePolymorphismPropagationItem> Items { get; }

    /// <summary>
    /// Количество наследников, значения которых изменятся.
    /// </summary>
    public int AffectedLeaveCount => Items.Count;

    /// <summary>
    /// Общее количество изменяемых атрибутов.
    /// </summary>
    public int ChangedAttributeCount { get; }
}
