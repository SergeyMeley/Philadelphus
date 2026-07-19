using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Presentation.Models.LeavePolymorphism;

namespace Philadelphus.Presentation.Services.Interfaces;

/// <summary>
/// Координирует интерактивную обработку изменения листа,
/// у которого могут быть разрешённые полиморфные наследники.
/// </summary>
public interface ILeavePolymorphismChangeCoordinator
{
    /// <summary>
    /// Запрашивает подтверждение и заполняет унаследованные атрибуты
    /// значениями выбранного родительского листа.
    /// </summary>
    /// <param name="recipientLeave">Лист-получатель.</param>
    /// <param name="parentLeave">Выбранный родительский лист.</param>
    /// <returns>Результат подтверждения и список изменённых атрибутов.</returns>
    Task<LeavePolymorphismManualFillResult> FillFromParentAsync(
        TreeLeaveModel recipientLeave,
        TreeLeaveModel parentLeave);

    /// <summary>
    /// Без дополнительного подтверждения создаёт отсутствующую цепочку родителей
    /// и обновляет её runtime-связи.
    /// </summary>
    /// <param name="childLeave">Лист, для которого создаётся цепочка.</param>
    /// <returns>Результат со списком созданных листов.</returns>
    LeavePolymorphismChangeResult CreateParentChain(TreeLeaveModel childLeave);

    /// <summary>
    /// Запрашивает подтверждение, применяет выбранную ветку каскада
    /// и пересчитывает runtime-связь самого изменённого листа.
    /// </summary>
    /// <param name="changedLeave">Изменённый лист.</param>
    /// <returns>Результат каскада и список созданных заменяющих листов.</returns>
    Task<LeavePolymorphismChangeResult> HandleChangedLeaveAsync(TreeLeaveModel changedLeave);
}
