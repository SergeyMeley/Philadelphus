using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;

/// <summary>
/// Нормализованный набор значений атрибутов, сопоставленных по DeclaringUuid.
/// </summary>
internal sealed class LeaveAttributeValueSignature
{
    private readonly IReadOnlyDictionary<Guid, AttributeValue> _values;

    private LeaveAttributeValueSignature(
        bool isValid,
        IReadOnlyDictionary<Guid, AttributeValue> values)
    {
        IsValid = isValid;
        _values = values;
    }

    /// <summary>
    /// Возвращает <see langword="true"/>, если все значения разрешены и набор
    /// атрибутов может участвовать в поиске совпадений.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Возвращает идентификаторы объявлений атрибутов, входящих в сигнатуру.
    /// </summary>
    public IEnumerable<Guid> DeclaringUuids => _values.Keys;

    /// <summary>
    /// Создаёт нормализованную сигнатуру из значений атрибутов.
    /// </summary>
    /// <param name="attributes">Атрибуты, сопоставляемые по <c>DeclaringUuid</c>.</param>
    /// <returns>Сигнатура с признаком возможности дальнейшего сравнения.</returns>
    public static LeaveAttributeValueSignature Create(
        IEnumerable<ElementAttributeModel> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        var values = new Dictionary<Guid, AttributeValue>();
        foreach (var attribute in attributes)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (HasInvalidReference(attribute)
                || values.TryAdd(attribute.DeclaringUuid, CreateValue(attribute)) == false)
            {
                return new(false, values);
            }
        }

        return new(true, values);
    }

    /// <summary>
    /// Создаёт нормализованную сигнатуру из независимых черновиков значений.
    /// </summary>
    /// <param name="drafts">Черновики, сопоставляемые по <c>DeclaringUuid</c>.</param>
    /// <returns>Сигнатура с признаком возможности дальнейшего сравнения.</returns>
    public static LeaveAttributeValueSignature Create(
        IEnumerable<LeaveAttributeValueDraft> drafts)
    {
        ArgumentNullException.ThrowIfNull(drafts);

        var values = new Dictionary<Guid, AttributeValue>();
        foreach (var draft in drafts)
        {
            ArgumentNullException.ThrowIfNull(draft);
            if (draft.MatchesAnyValue
                || values.TryAdd(draft.DeclaringUuid, CreateValue(draft)) == false)
                return new(false, values);
        }

        return new(true, values);
    }

    /// <summary>
    /// Сравнивает две валидные сигнатуры по объявлениям и UUID значений.
    /// </summary>
    /// <param name="other">Сигнатура потенциального совпадения.</param>
    /// <returns>
    /// <see langword="true"/>, если состав атрибутов совпадает, одиночные значения
    /// равны, а коллекции содержат одинаковые UUID без учёта порядка.
    /// </returns>
    public bool Matches(LeaveAttributeValueSignature other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (IsValid == false || other.IsValid == false || _values.Count != other._values.Count)
            return false;

        return _values.All(pair =>
            other._values.TryGetValue(pair.Key, out var otherValue)
            && pair.Value.IsCollection == otherValue.IsCollection
            && pair.Value.ValueUuids.SetEquals(otherValue.ValueUuids));
    }

    private static bool HasInvalidReference(ElementAttributeModel attribute) =>
        attribute.ValueType == null
        || string.IsNullOrEmpty(attribute.ValueTypeReferenceErrorCode) == false
        || string.IsNullOrEmpty(attribute.ValueFormulaErrorCode) == false;

    private static AttributeValue CreateValue(ElementAttributeModel attribute)
    {
        // Даже пустое значение представляется множеством: это позволяет считать его
        // полноценным значением и использовать одну операцию сравнения для обоих типов.
        IEnumerable<Guid> valueUuids = attribute.IsCollectionValue
            ? attribute.Values.Select(x => x.Uuid)
            : attribute.Value == null
                ? []
                : [attribute.Value.Uuid];

        return new(attribute.IsCollectionValue, valueUuids.ToHashSet());
    }

    private static AttributeValue CreateValue(LeaveAttributeValueDraft draft)
    {
        IEnumerable<Guid> valueUuids = draft.IsCollection
            ? draft.ValueUuids
            : draft.ValueUuid is Guid valueUuid
                ? [valueUuid]
                : [];

        return new(draft.IsCollection, valueUuids.ToHashSet());
    }

    private sealed record AttributeValue(
        bool IsCollection,
        HashSet<Guid> ValueUuids);
}
