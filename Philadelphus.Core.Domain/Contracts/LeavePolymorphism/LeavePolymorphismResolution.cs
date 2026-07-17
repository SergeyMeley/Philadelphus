using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Неизменяемый снимок результата поиска полиморфного родителя.
/// </summary>
/// <param name="ChildLeave">Лист, для которого выполнялся поиск.</param>
/// <param name="Status">Итоговый статус разрешения связи.</param>
/// <param name="ParentLeave">Однозначно найденный родитель.</param>
/// <param name="Candidates">Совпавшие по значениям кандидаты.</param>
public sealed record LeavePolymorphismResolution(
    TreeLeaveModel ChildLeave,
    LeavePolymorphismStatus Status,
    TreeLeaveModel? ParentLeave,
    IReadOnlyList<TreeLeaveModel> Candidates);
