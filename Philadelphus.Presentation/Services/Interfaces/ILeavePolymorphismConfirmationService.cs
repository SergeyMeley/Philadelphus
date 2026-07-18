using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Presentation.Services.Interfaces;

/// <summary>
/// Подтверждает интерактивные операции, перезаписывающие значения полиморфных наследников.
/// </summary>
public interface ILeavePolymorphismConfirmationService
{
    /// <summary>
    /// Подтверждает заполнение унаследованных атрибутов выбранным родительским листом.
    /// </summary>
    /// <param name="recipientLeave">Лист, значения которого будут перезаписаны.</param>
    /// <param name="changedAttributeCount">Количество изменяемых атрибутов.</param>
    /// <returns>true, если операция подтверждена; иначе false.</returns>
    Task<bool> ConfirmManualFillAsync(
        TreeLeaveModel recipientLeave,
        int changedAttributeCount);

    /// <summary>
    /// Подтверждает транзитивное обновление разрешённых наследников.
    /// </summary>
    /// <param name="changedParentLeave">Изменённый родительский лист.</param>
    /// <param name="affectedLeaveCount">Количество затронутых наследников.</param>
    /// <param name="changedAttributeCount">Общее количество изменяемых атрибутов.</param>
    /// <returns>true, если операция подтверждена; иначе false.</returns>
    Task<bool> ConfirmPropagationAsync(
        TreeLeaveModel changedParentLeave,
        int affectedLeaveCount,
        int changedAttributeCount);
}
