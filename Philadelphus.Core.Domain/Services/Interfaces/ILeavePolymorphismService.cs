using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Вычисляет и восстанавливает runtime-связи полиморфных листов.
/// </summary>
public interface ILeavePolymorphismService
{
    LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave);

    IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(IEnumerable<TreeLeaveModel> leaves);
}
