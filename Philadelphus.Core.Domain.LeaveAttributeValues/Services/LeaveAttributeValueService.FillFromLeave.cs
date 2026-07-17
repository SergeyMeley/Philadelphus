using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

public sealed partial class LeaveAttributeValueService
{
    /// <summary>
    /// Полностью заменяет выбранные значения атрибутов целевого листа
    /// материализованными значениями исходного листа.
    /// </summary>
    /// <param name="targetLeave">Заполняемый лист.</param>
    /// <param name="sourceLeave">Лист-источник значений.</param>
    /// <param name="declaringUuids">Идентификаторы объявлений заполняемых атрибутов.</param>
    /// <returns>Атрибуты целевого листа, фактически изменённые операцией.</returns>
    public LeaveAttributeFillResult FillFromLeave(
        TreeLeaveModel targetLeave,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids)
    {
        ArgumentNullException.ThrowIfNull(targetLeave);
        ArgumentNullException.ThrowIfNull(sourceLeave);
        ArgumentNullException.ThrowIfNull(declaringUuids);

        if (ReferenceEquals(targetLeave, sourceLeave))
            return new([]);

        var requestedUuids = declaringUuids.ToHashSet();
        var sourceAttributes = sourceLeave.Attributes
            .Where(x => requestedUuids.Contains(x.DeclaringUuid))
            .ToDictionary(x => x.DeclaringUuid);
        var changedAttributes = new List<ElementAttributeModel>();

        foreach (var targetAttribute in targetLeave.Attributes
                     .Where(x => requestedUuids.Contains(x.DeclaringUuid)))
        {
            if (sourceAttributes.TryGetValue(targetAttribute.DeclaringUuid, out var sourceAttribute) == false)
                continue;

            EnsureCompatibleSource(targetAttribute, sourceAttribute);
            var changed = targetAttribute.IsCollectionValue
                ? FillCollection(targetAttribute, sourceAttribute)
                : FillScalar(targetAttribute, sourceAttribute);

            if (changed)
                changedAttributes.Add(targetAttribute);
        }

        return new(changedAttributes);
    }

    /// <summary>
    /// Записывает одиночное значение формулой-ссылкой либо полностью очищает его.
    /// </summary>
    private static bool FillScalar(
        ElementAttributeModel targetAttribute,
        ElementAttributeModel sourceAttribute)
    {
        var sourceValue = sourceAttribute.Value;
        var expectedFormula = sourceValue == null
            ? string.Empty
            : FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(sourceValue.Uuid);

        if (ScalarMatches(targetAttribute, sourceValue, expectedFormula))
            return false;

        if (sourceValue == null)
            targetAttribute.ClearFormulaValue();
        else
            targetAttribute.AssignValueAsFormula(sourceValue);

        if (ScalarMatches(targetAttribute, sourceValue, expectedFormula) == false)
            throw CreateFillException(targetAttribute, "одиночное значение отклонено политикой модели");

        return true;
    }

    /// <summary>
    /// Полностью заменяет коллекцию, сохраняя порядок значений источника.
    /// </summary>
    private static bool FillCollection(
        ElementAttributeModel targetAttribute,
        ElementAttributeModel sourceAttribute)
    {
        var sourceValues = sourceAttribute.Values.ToList();
        if (string.IsNullOrEmpty(targetAttribute.ValuesReferenceErrorCode)
            && ValuesMatch(targetAttribute.Values, sourceValues))
        {
            return false;
        }

        if (targetAttribute.ClearValuesCollection() == false)
            throw CreateFillException(targetAttribute, "коллекцию значений очистить не удалось");

        foreach (var value in sourceValues)
        {
            if (targetAttribute.TryAddValueToValuesCollection(value) == false)
                throw CreateFillException(targetAttribute, $"значение '{value.Name}' [{value.Uuid}] добавить не удалось");
        }

        return true;
    }

    /// <summary>
    /// Проверяет равенство материализованного значения, формулы-ссылки и ошибки вычисления.
    /// </summary>
    private static bool ScalarMatches(
        ElementAttributeModel targetAttribute,
        TreeLeaveModel? sourceValue,
        string expectedFormula) =>
        targetAttribute.Value?.Uuid == sourceValue?.Uuid
        && string.Equals(targetAttribute.ValueFormula, expectedFormula, StringComparison.Ordinal)
        && string.IsNullOrEmpty(targetAttribute.ValueFormulaErrorCode);

    /// <summary>
    /// Сравнивает коллекции как множества UUID без учёта порядка.
    /// </summary>
    private static bool ValuesMatch(
        IEnumerable<TreeLeaveModel> first,
        IEnumerable<TreeLeaveModel> second) =>
        first.Select(x => x.Uuid).ToHashSet().SetEquals(second.Select(x => x.Uuid));

    /// <summary>
    /// Проверяет, что источник разрешён и использует тот же вид значения.
    /// </summary>
    private static void EnsureCompatibleSource(
        ElementAttributeModel targetAttribute,
        ElementAttributeModel sourceAttribute)
    {
        if (targetAttribute.IsCollectionValue != sourceAttribute.IsCollectionValue)
            throw CreateFillException(targetAttribute, "вид значения источника не совпадает с целевым");

        if (LeaveAttributeValueSignature.Create([sourceAttribute]).IsValid == false)
            throw CreateFillException(targetAttribute, "значение источника не разрешено");
    }

    /// <summary>
    /// Создаёт диагностическое исключение с идентификатором объявления атрибута.
    /// </summary>
    private static InvalidOperationException CreateFillException(
        ElementAttributeModel attribute,
        string reason) =>
        new($"Не удалось заполнить атрибут '{attribute.Name}' [{attribute.LocalUuid}]: {reason}.");
}
