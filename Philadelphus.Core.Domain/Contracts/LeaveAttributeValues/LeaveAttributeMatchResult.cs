using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;

/// <summary>
/// Результат универсального поиска листов по значениям явно переданных атрибутов.
/// </summary>
public sealed record LeaveAttributeMatchResult(
    bool IsValid,
    IReadOnlyList<TreeLeaveModel> Matches);
