using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.LeavePolymorphism.Models;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <inheritdoc />
    public LeavePolymorphismPropagationPlan BuildPropagationPlan(
        TreeLeaveModel changedParentLeave)
    {
        ArgumentNullException.ThrowIfNull(changedParentLeave);

        if (IsDeleted(changedParentLeave))
            throw new InvalidOperationException("Невозможно построить план от удалённого листа.");

        var items = new List<LeavePolymorphismPropagationItem>();
        var projections = new Dictionary<Guid,
            Dictionary<Guid, LeavePolymorphismProjectedAttributeValue>>
        {
            [changedParentLeave.Uuid] = CreateProjection(changedParentLeave)
        };
        var visitedUuids = new HashSet<Guid> { changedParentLeave.Uuid };
        var queue = new Queue<TreeLeaveModel>();
        queue.Enqueue(changedParentLeave);

        while (queue.TryDequeue(out var sourceLeave))
        {
            var sourceProjection = projections[sourceLeave.Uuid];

            // ToList фиксирует текущую runtime-топологию до последующего пересчёта связей.
            foreach (var targetLeave in sourceLeave.PolymorphicChildLeaves.ToList())
            {
                if (IsDeleted(targetLeave))
                    continue;

                if (visitedUuids.Add(targetLeave.Uuid) == false)
                {
                    throw new InvalidOperationException(
                        $"Лист '{targetLeave.Name}' [{targetLeave.Uuid}] повторяется в цепочке обновления.");
                }

                var targetProjection = CreateProjection(targetLeave);
                var declaringUuids = sourceLeave.ParentNode.Attributes
                    .Select(x => x.DeclaringUuid)
                    .ToList();
                var changedAttributeCount = ApplyProjection(
                    sourceLeave,
                    targetLeave,
                    sourceProjection,
                    targetProjection,
                    declaringUuids);

                if (changedAttributeCount > 0)
                {
                    items.Add(new(
                        sourceLeave,
                        targetLeave,
                        declaringUuids,
                        changedAttributeCount));
                }

                projections[targetLeave.Uuid] = targetProjection;
                queue.Enqueue(targetLeave);
            }
        }

        return new(changedParentLeave, items);
    }

    /// <summary>
    /// Создаёт проекцию всех эффективных атрибутов листа.
    /// </summary>
    private static Dictionary<Guid, LeavePolymorphismProjectedAttributeValue> CreateProjection(
        TreeLeaveModel leave)
    {
        var result = new Dictionary<Guid, LeavePolymorphismProjectedAttributeValue>();
        foreach (var attribute in leave.Attributes)
        {
            if (result.TryAdd(
                    attribute.DeclaringUuid,
                    new LeavePolymorphismProjectedAttributeValue(attribute)) == false)
            {
                throw new InvalidOperationException(
                    $"Лист '{leave.Name}' [{leave.Uuid}] содержит повторяющееся объявление "
                    + $"атрибута [{attribute.DeclaringUuid}].");
            }
        }

        return result;
    }

    /// <summary>
    /// Переносит значения родительского уровня в проекцию наследника и считает изменения.
    /// </summary>
    private static int ApplyProjection(
        TreeLeaveModel sourceLeave,
        TreeLeaveModel targetLeave,
        IReadOnlyDictionary<Guid, LeavePolymorphismProjectedAttributeValue> sourceProjection,
        IDictionary<Guid, LeavePolymorphismProjectedAttributeValue> targetProjection,
        IEnumerable<Guid> declaringUuids)
    {
        var changedCount = 0;
        foreach (var declaringUuid in declaringUuids)
        {
            if (sourceProjection.TryGetValue(declaringUuid, out var sourceValue) == false
                || targetProjection.TryGetValue(declaringUuid, out var targetValue) == false
                || sourceValue.IsCompatibleWith(targetValue) == false)
            {
                throw new InvalidOperationException(
                    $"Невозможно спроецировать атрибут [{declaringUuid}] из листа "
                    + $"'{sourceLeave.Name}' [{sourceLeave.Uuid}] в лист "
                    + $"'{targetLeave.Name}' [{targetLeave.Uuid}].");
            }

            if (sourceValue.HasSameValue(targetValue) == false)
                changedCount++;

            targetProjection[declaringUuid] = sourceValue;
        }

        return changedCount;
    }
}
