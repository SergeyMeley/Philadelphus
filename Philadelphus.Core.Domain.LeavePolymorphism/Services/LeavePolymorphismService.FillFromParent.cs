using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <inheritdoc />
    public int CountFillFromParentChanges(
        TreeLeaveModel childLeave,
        TreeLeaveModel parentLeave)
    {
        var declaringUuids = GetFillDeclaringUuids(childLeave, parentLeave);
        return _attributeValueService.CountFillChanges(
            childLeave,
            parentLeave,
            declaringUuids);
    }

    /// <inheritdoc />
    public LeaveAttributeFillResult FillFromParent(
        TreeLeaveModel childLeave,
        TreeLeaveModel parentLeave)
    {
        var declaringUuids = GetFillDeclaringUuids(childLeave, parentLeave);
        return _attributeValueService.FillFromLeave(
            childLeave,
            parentLeave,
            declaringUuids);
    }

    /// <summary>
    /// Проверяет положение листов в дереве и возвращает объявления только
    /// непосредственного родительского уровня.
    /// </summary>
    private static IReadOnlySet<Guid> GetFillDeclaringUuids(
        TreeLeaveModel childLeave,
        TreeLeaveModel parentLeave)
    {
        ArgumentNullException.ThrowIfNull(childLeave);
        ArgumentNullException.ThrowIfNull(parentLeave);

        var expectedParentNode = childLeave.ParentNode.ParentNode;
        if (expectedParentNode == null
            || expectedParentNode.Uuid != parentLeave.ParentNode.Uuid
            || IsDeleted(childLeave)
            || IsDeleted(parentLeave))
        {
            throw new InvalidOperationException(
                $"Лист '{parentLeave.Name}' [{parentLeave.Uuid}] не может быть "
                + $"полиморфным родителем листа '{childLeave.Name}' [{childLeave.Uuid}].");
        }

        return expectedParentNode.Attributes
            .Where(x => IsPolymorphismAttribute(x) == false)
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
    }
}
