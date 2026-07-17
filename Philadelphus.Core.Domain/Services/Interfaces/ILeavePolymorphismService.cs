using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Вычисляет и восстанавливает runtime-связи полиморфных листов.
/// </summary>
public interface ILeavePolymorphismService
{
    /// <summary>
    /// Вычисляет родителя одного листа и обновляет его runtime-связь.
    /// </summary>
    /// <param name="childLeave">Лист, для которого выполняется поиск.</param>
    /// <returns>Статус разрешения и найденные кандидаты.</returns>
    LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave);

    /// <summary>
    /// Последовательно перестраивает runtime-связи переданных листов.
    /// </summary>
    /// <param name="leaves">Листы для восстановления связей.</param>
    /// <returns>Результат разрешения каждого переданного листа.</returns>
    IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(IEnumerable<TreeLeaveModel> leaves);
}
