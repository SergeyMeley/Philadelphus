using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Неизменяемый снимок результата поиска полиморфного родителя.
/// </summary>
public sealed record LeavePolymorphismResolution(
    TreeLeaveModel ChildLeave,
    LeavePolymorphismStatus Status,
    TreeLeaveModel? ParentLeave,
    IReadOnlyList<TreeLeaveModel> Candidates);
