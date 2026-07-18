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
    /// Запрашивает подтверждение, применяет выбранную ветку каскада
    /// и пересчитывает runtime-связь самого изменённого листа.
    /// </summary>
    /// <param name="changedLeave">Изменённый лист.</param>
    /// <returns>Результат каскада и список созданных заменяющих листов.</returns>
    Task<LeavePolymorphismChangeResult> HandleChangedLeaveAsync(TreeLeaveModel changedLeave);
}
