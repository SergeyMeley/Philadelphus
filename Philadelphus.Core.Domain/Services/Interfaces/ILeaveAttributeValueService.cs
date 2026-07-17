using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Выполняет переиспользуемые операции со значениями атрибутов листов.
/// </summary>
public interface ILeaveAttributeValueService
{
    LeaveAttributeMatchResult FindMatches(
        IEnumerable<ElementAttributeModel> expectedAttributes,
        IEnumerable<TreeLeaveModel> candidates);

    LeaveAttributeFillResult FillFromLeave(
        TreeLeaveModel targetLeave,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids);

    TreeLeaveModel CreateLeave(
        TreeNodeModel parentNode,
        IEnumerable<ElementAttributeModel> sourceAttributes);
}
