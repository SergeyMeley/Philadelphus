using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

public sealed partial class LeaveAttributeValueService
{
    /// <summary>
    /// Создаёт пользовательский лист по полному набору независимых черновиков
    /// без немедленного сохранения в репозитории.
    /// </summary>
    /// <param name="parentNode">Родительский узел нового листа.</param>
    /// <param name="sourceValues">Черновики значений всех нерuntime-атрибутов.</param>
    /// <returns>Созданный лист в состоянии <c>Initialized</c>.</returns>
    public TreeLeaveModel CreateLeave(
        TreeNodeModel parentNode,
        IEnumerable<LeaveAttributeValueDraft> sourceValues)
    {
        ArgumentNullException.ThrowIfNull(parentNode);
        ArgumentNullException.ThrowIfNull(sourceValues);
        if (parentNode is SystemBaseTreeNodeModel)
        {
            throw new InvalidOperationException(
                "Системные листья необходимо создавать через CreateSystemValue.");
        }

        var drafts = sourceValues.ToList();
        var resolvedValues = ResolveDraftValues(parentNode, drafts);
        var matchResult = FindMatches(
            drafts,
            parentNode.ChildLeaves.Where(IsActive));
        if (matchResult.Status == LeaveAttributeMatchStatus.Invalid)
            throw new InvalidOperationException("Невозможно создать лист: значения кандидатов не разрешены.");
        if (matchResult.Status != LeaveAttributeMatchStatus.NotFound)
        {
            throw new InvalidOperationException(
                $"Лист с переданными значениями уже представлен "
                + $"{matchResult.Matches.Count} активными кандидатами.");
        }

        var createdLeave = _repositoryService.CreateTreeLeave(
            parentNode,
            needAutoName: true,
            withoutInfoNotifications: true)
            ?? throw new InvalidOperationException(
                $"Не удалось создать лист в узле '{parentNode.Name}' [{parentNode.Uuid}].");

        FillDraftValues(createdLeave, resolvedValues);
        return createdLeave;
    }

    private static IReadOnlyList<ResolvedDraftValue> ResolveDraftValues(
        TreeNodeModel parentNode,
        IReadOnlyCollection<LeaveAttributeValueDraft> drafts)
    {
        if (LeaveAttributeValueSignature.Create(drafts).IsValid == false)
            throw new InvalidOperationException("Невозможно создать лист: набор черновиков невалиден.");

        var declarations = parentNode.Attributes
            .Where(x => x.IsRuntime == false)
            .ToDictionary(x => x.DeclaringUuid);
        if (declarations.Count != drafts.Count)
            throw new InvalidOperationException("Невозможно создать лист: передан неполный набор атрибутов.");

        var result = new List<ResolvedDraftValue>();
        foreach (var draft in drafts)
        {
            if (declarations.TryGetValue(draft.DeclaringUuid, out var declaration) == false)
            {
                throw new InvalidOperationException(
                    $"Объявление атрибута [{draft.DeclaringUuid}] отсутствует в родительском узле.");
            }

            if (declaration.IsCollectionValue != draft.IsCollection)
            {
                throw new InvalidOperationException(
                    $"Вид значения атрибута [{draft.DeclaringUuid}] не совпадает с черновиком.");
            }

            if (declaration.ValueType == null
                || string.IsNullOrEmpty(declaration.ValueTypeReferenceErrorCode) == false)
            {
                throw new InvalidOperationException(
                    $"Тип значения атрибута [{draft.DeclaringUuid}] не разрешён.");
            }

            var requestedUuids = draft.IsCollection
                ? draft.ValueUuids
                : draft.ValueUuid is Guid valueUuid
                    ? new HashSet<Guid> { valueUuid }
                    : new HashSet<Guid>();
            var resolved = declaration.ValueType.ChildLeaves
                .Where(IsActive)
                .Where(x => requestedUuids.Contains(x.Uuid))
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Uuid)
                .ToList();
            if (resolved.Count != requestedUuids.Count)
            {
                throw new InvalidOperationException(
                    $"Не все значения атрибута [{draft.DeclaringUuid}] разрешены в его типе.");
            }

            result.Add(new(draft.DeclaringUuid, draft.IsCollection, resolved));
        }

        return result;
    }

    private static void FillDraftValues(
        TreeLeaveModel targetLeave,
        IEnumerable<ResolvedDraftValue> resolvedValues)
    {
        var targetAttributes = targetLeave.Attributes.ToDictionary(x => x.DeclaringUuid);
        foreach (var resolvedValue in resolvedValues)
        {
            var targetAttribute = targetAttributes[resolvedValue.DeclaringUuid];
            if (resolvedValue.IsCollection)
            {
                if (CollectionMatches(targetAttribute, resolvedValue.Values))
                    continue;

                targetAttribute.ClearValuesCollection();
                if (targetAttribute.Values.Count != 0)
                    throw CreateFillException(targetAttribute, "коллекцию значений очистить не удалось");
                foreach (var value in resolvedValue.Values)
                {
                    if (targetAttribute.TryAddValueToValuesCollection(value) == false)
                        throw CreateFillException(targetAttribute, $"значение [{value.Uuid}] добавить не удалось");
                }

                targetAttribute.AssignValuesAsFormula();
                if (CollectionMatches(targetAttribute, resolvedValue.Values) == false)
                    throw CreateFillException(targetAttribute, "коллекционное значение отклонено политикой модели");

                continue;
            }

            var scalarValue = resolvedValue.Values.SingleOrDefault();
            var expectedFormula = scalarValue == null
                ? string.Empty
                : FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(scalarValue.Uuid);
            if (ScalarMatches(targetAttribute, scalarValue, expectedFormula))
                continue;

            if (scalarValue == null)
                targetAttribute.ClearFormulaValue();
            else
                targetAttribute.AssignValueAsFormula(scalarValue);

            if (ScalarMatches(targetAttribute, scalarValue, expectedFormula) == false)
                throw CreateFillException(targetAttribute, "одиночное значение отклонено политикой модели");
        }
    }

    private sealed record ResolvedDraftValue(
        Guid DeclaringUuid,
        bool IsCollection,
        IReadOnlyList<TreeLeaveModel> Values);
}
