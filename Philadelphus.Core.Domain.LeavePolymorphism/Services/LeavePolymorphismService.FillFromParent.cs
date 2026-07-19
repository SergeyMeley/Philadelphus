using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <inheritdoc />
    public int CountFillFromParentChanges(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave)
    {
        var declaringUuids = GetFillDeclaringUuids(childOwner, parentLeave);
        return _attributeValueService.CountFillChanges(
            childOwner,
            parentLeave,
            declaringUuids);
    }

    /// <inheritdoc />
    public LeaveAttributeFillResult FillFromParent(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave)
    {
        var declaringUuids = GetFillDeclaringUuids(childOwner, parentLeave);
        return _attributeValueService.FillFromLeave(
            childOwner,
            parentLeave,
            declaringUuids);
    }

    /// <summary>
    /// Проверяет положение листов в дереве и возвращает объявления только
    /// непосредственного родительского уровня.
    /// </summary>
    private static IReadOnlySet<Guid> GetFillDeclaringUuids(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave)
    {
        ArgumentNullException.ThrowIfNull(childOwner);
        ArgumentNullException.ThrowIfNull(parentLeave);

        var expectedParentNode = childOwner switch
        {
            TreeLeaveModel leave => leave.ParentNode.ParentNode,
            TreeNodeModel node => node.ParentNode,
            _ => null
        };
        if (expectedParentNode == null
            || expectedParentNode.Uuid != parentLeave.ParentNode.Uuid
            || childOwner is TreeLeaveModel childLeave && IsDeleted(childLeave)
            || childOwner is TreeNodeModel childNode && IsDeleted(childNode)
            || IsDeleted(parentLeave))
        {
            throw new InvalidOperationException(
                $"Лист '{parentLeave.Name}' [{parentLeave.Uuid}] не может быть "
                + $"полиморфным родителем элемента '{childOwner.Name}' [{childOwner.Uuid}].");
        }

        return expectedParentNode.Attributes
            .Where(x => IsPolymorphismAttribute(x) == false)
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
    }
}
