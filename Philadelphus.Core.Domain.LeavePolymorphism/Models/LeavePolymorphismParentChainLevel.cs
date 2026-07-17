using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Models;

/// <summary>
/// Описывает один предварительно проверенный уровень создаваемой цепочки.
/// </summary>
/// <param name="ParentNode">Узел, в котором должен находиться родительский лист.</param>
/// <param name="SourceAttributes">Значения атрибутов для возможного создания листа.</param>
/// <param name="ExistingParentLeave">Единственный существующий кандидат или <see langword="null"/>.</param>
internal sealed record LeavePolymorphismParentChainLevel(
    TreeNodeModel ParentNode,
    IReadOnlyList<ElementAttributeModel> SourceAttributes,
    TreeLeaveModel? ExistingParentLeave);
