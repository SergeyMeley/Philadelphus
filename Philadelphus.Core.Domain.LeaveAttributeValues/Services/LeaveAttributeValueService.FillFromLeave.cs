using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

public sealed partial class LeaveAttributeValueService
{
    /// <summary>
    /// Полностью заменяет выбранные значения атрибутов целевого элемента
    /// материализованными значениями исходного листа.
    /// </summary>
    /// <param name="targetOwner">Заполняемый узел или лист.</param>
    /// <param name="sourceLeave">Лист-источник значений.</param>
    /// <param name="declaringUuids">Идентификаторы объявлений заполняемых атрибутов.</param>
    /// <returns>Атрибуты целевого элемента, фактически изменённые операцией.</returns>
    public LeaveAttributeFillResult FillFromLeave(
        IAttributeOwnerModel targetOwner,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids)
    {
        ArgumentNullException.ThrowIfNull(targetOwner);
        ArgumentNullException.ThrowIfNull(sourceLeave);
        ArgumentNullException.ThrowIfNull(declaringUuids);

        if (ReferenceEquals(targetOwner, sourceLeave))
            return new([]);

        var requestedUuids = declaringUuids.ToHashSet();
        var changedAttributes = FindChangedAttributes(
            targetOwner,
            sourceLeave.Attributes,
            requestedUuids);
        return FillAttributes(changedAttributes, sourceLeave.Attributes);
    }

    /// <inheritdoc />
    public int CountFillChanges(
        IAttributeOwnerModel targetOwner,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids)
    {
        ArgumentNullException.ThrowIfNull(targetOwner);
        ArgumentNullException.ThrowIfNull(sourceLeave);
        ArgumentNullException.ThrowIfNull(declaringUuids);

        if (ReferenceEquals(targetOwner, sourceLeave))
            return 0;

        return FindChangedAttributes(
                targetOwner,
                sourceLeave.Attributes,
                declaringUuids.ToHashSet())
            .Count;
    }

    /// <summary>
    /// Находит атрибуты, которые заполнение действительно изменит, и заранее
    /// проверяет совместимость всех источников до начала мутаций.
    /// </summary>
    private static IReadOnlyList<ElementAttributeModel> FindChangedAttributes(
        IAttributeOwnerModel targetOwner,
        IEnumerable<ElementAttributeModel> sourceAttributes,
        IReadOnlySet<Guid> requestedUuids)
    {
        var sourceAttributesByUuid = sourceAttributes
            .Where(x => requestedUuids.Contains(x.DeclaringUuid))
            .ToDictionary(x => x.DeclaringUuid);
        var changedAttributes = new List<ElementAttributeModel>();

        foreach (var targetAttribute in targetOwner.Attributes
                     .Where(x => requestedUuids.Contains(x.DeclaringUuid)))
        {
            if (sourceAttributesByUuid.TryGetValue(
                    targetAttribute.DeclaringUuid,
                    out var sourceAttribute) == false)
            {
                continue;
            }

            EnsureCompatibleSource(targetAttribute, sourceAttribute);
            if (RequiresFill(targetAttribute, sourceAttribute))
                changedAttributes.Add(targetAttribute);
        }

        return changedAttributes;
    }

    /// <summary>
    /// Заполняет предварительно проверенные целевые атрибуты.
    /// </summary>
    private static LeaveAttributeFillResult FillAttributes(
        IReadOnlyList<ElementAttributeModel> changedAttributes,
        IEnumerable<ElementAttributeModel> sourceAttributes)
    {
        var sourceAttributesByUuid = sourceAttributes
            .ToDictionary(x => x.DeclaringUuid);

        foreach (var targetAttribute in changedAttributes)
        {
            var sourceAttribute = sourceAttributesByUuid[targetAttribute.DeclaringUuid];
            if (targetAttribute.IsCollectionValue)
                FillCollection(targetAttribute, sourceAttribute);
            else
                FillScalar(targetAttribute, sourceAttribute);
        }

        return new(changedAttributes);
    }

    /// <summary>
    /// Проверяет, отличается ли текущее представление значения от результата заполнения.
    /// </summary>
    private static bool RequiresFill(
        ElementAttributeModel targetAttribute,
        ElementAttributeModel sourceAttribute)
    {
        if (targetAttribute.IsCollectionValue)
        {
            return string.IsNullOrEmpty(targetAttribute.ValuesReferenceErrorCode) == false
                || ValuesMatch(targetAttribute.Values, sourceAttribute.Values) == false;
        }

        var sourceValue = sourceAttribute.Value;
        var expectedFormula = sourceValue == null
            ? string.Empty
            : FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(sourceValue.Uuid);
        return ScalarMatches(targetAttribute, sourceValue, expectedFormula) == false;
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

        if (targetAttribute.ValueType?.Uuid != sourceAttribute.ValueType?.Uuid)
            throw CreateFillException(targetAttribute, "тип значения источника не совпадает с целевым");

        if (LeaveAttributeValueSignature.Create([sourceAttribute]).IsValid == false)
            throw CreateFillException(targetAttribute, "значение источника не разрешено");
    }

    /// <summary>
    /// Создаёт диагностическое исключение с локальным идентификатором атрибута.
    /// </summary>
    private static InvalidOperationException CreateFillException(
        ElementAttributeModel attribute,
        string reason) =>
        new($"Не удалось заполнить атрибут '{attribute.Name}' [{attribute.LocalUuid}]: {reason}.");
}
