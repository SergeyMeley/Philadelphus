using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;

/// <summary>
/// Результат универсального поиска листов по значениям явно переданных атрибутов.
/// </summary>
/// <param name="Status">Итоговый статус поиска.</param>
/// <param name="Matches">Совпавшие по значениям листы.</param>
public sealed record LeaveAttributeMatchResult(
    LeaveAttributeMatchStatus Status,
    IReadOnlyList<TreeLeaveModel> Matches)
{
    /// <summary>
    /// Возвращает признак возможности сравнения значений.
    /// </summary>
    public bool IsValid => Status != LeaveAttributeMatchStatus.Invalid;

    /// <summary>
    /// Возвращает однозначно найденный лист.
    /// </summary>
    public TreeLeaveModel? ResolvedMatch =>
        Status == LeaveAttributeMatchStatus.Resolved ? Matches.Single() : null;
}
