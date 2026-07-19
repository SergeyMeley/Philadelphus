using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.LeavePolymorphism.Models;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <inheritdoc />
    public IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeNodeModel childNode)
    {
        ArgumentNullException.ThrowIfNull(childNode);

        if (IsDeleted(childNode) || childNode.ParentNode == null)
            throw new InvalidOperationException("Невозможно создать родительский лист для недопустимого узла.");

        var declaringUuids = childNode.ParentNode.Attributes
            .Where(x => IsPolymorphismAttribute(x) == false)
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
        var sourceAttributes = GetExpectedParentAttributes(childNode, declaringUuids);
        var matchResult = FindParentCandidates(childNode, childNode.ParentNode);
        if (matchResult.IsValid == false || matchResult.Matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"Невозможно создать родительский лист узла '{childNode.Name}' [{childNode.Uuid}]: "
                + "значения атрибутов невалидны или неоднозначны.");
        }

        var createdLeaves = new List<TreeLeaveModel>();
        var parentLeave = matchResult.Matches.SingleOrDefault();
        if (parentLeave == null)
        {
            parentLeave = _attributeValueService.CreateLeave(
                childNode.ParentNode,
                sourceAttributes);
            createdLeaves.Add(parentLeave);
        }

        childNode.SetPolymorphicParentLeave(parentLeave);
        if (parentLeave.ParentNode.ParentNode != null)
            createdLeaves.AddRange(CreateParentChain(parentLeave));
        ResolveParent(childNode);
        return createdLeaves;
    }

    /// <inheritdoc />
    public IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeLeaveModel childLeave)
    {
        ArgumentNullException.ThrowIfNull(childLeave);

        var plan = BuildParentChainPlan(childLeave);
        var createdLeaves = new List<TreeLeaveModel>();
        var currentChild = childLeave;

        foreach (var level in plan)
        {
            var parentLeave = level.ExistingParentLeave;
            if (parentLeave == null)
            {
                parentLeave = _attributeValueService.CreateLeave(
                    level.ParentNode,
                    level.SourceAttributes);
                createdLeaves.Add(parentLeave);
            }

            // Повторная установка уже существующей связи является допустимой идемпотентной операцией.
            if (currentChild.SetPolymorphicParentLeave(parentLeave) == false
                && currentChild.PolymorphicParentLeave?.Uuid != parentLeave.Uuid)
            {
                throw new InvalidOperationException(
                    $"Не удалось связать лист '{currentChild.Name}' [{currentChild.Uuid}] "
                    + $"с полиморфным родителем '{parentLeave.Name}' [{parentLeave.Uuid}].");
            }

            currentChild = parentLeave;
        }

        return createdLeaves;
    }

    /// <summary>
    /// Проверяет всю цепочку до начала мутаций и определяет переиспользуемые листья.
    /// </summary>
    private IReadOnlyList<LeavePolymorphismParentChainLevel> BuildParentChainPlan(
        TreeLeaveModel childLeave)
    {
        if (IsDeleted(childLeave))
            throw CreateParentChainException(childLeave, null, LeavePolymorphismStatus.Invalid);

        var levels = new List<LeavePolymorphismParentChainLevel>();
        var parentNode = childLeave.ParentNode.ParentNode;

        while (parentNode != null)
        {
            var declaringUuids = parentNode.Attributes
                .Where(x => IsPolymorphismAttribute(x) == false)
                .Select(x => x.DeclaringUuid)
                .ToHashSet();
            var sourceAttributes = GetExpectedParentAttributes(childLeave, declaringUuids);
            var matchResult = FindParentCandidates(childLeave, parentNode);

            if (matchResult.IsValid == false)
                throw CreateParentChainException(
                    childLeave, parentNode, LeavePolymorphismStatus.Invalid);

            if (matchResult.Matches.Count > 1)
                throw CreateParentChainException(
                    childLeave, parentNode, LeavePolymorphismStatus.Ambiguous);

            levels.Add(new(
                parentNode,
                sourceAttributes,
                matchResult.Matches.SingleOrDefault()));
            parentNode = parentNode.ParentNode;
        }

        if (levels.Count == 0)
            throw CreateParentChainException(childLeave, null, LeavePolymorphismStatus.Invalid);

        return levels;
    }

    /// <summary>
    /// Создаёт диагностическое исключение для уровня, блокирующего создание цепочки.
    /// </summary>
    private static InvalidOperationException CreateParentChainException(
        TreeLeaveModel childLeave,
        TreeNodeModel? parentNode,
        LeavePolymorphismStatus status)
    {
        var level = parentNode == null
            ? "без родительского узла"
            : $"в узле '{parentNode.Name}' [{parentNode.Uuid}]";
        return new(
            $"Невозможно создать цепочку родителей листа '{childLeave.Name}' "
            + $"[{childLeave.Uuid}]: связь {level} имеет статус {status}.");
    }
}
