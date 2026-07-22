using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Models;

/// <summary>
/// Хранит нормализованный снимок материализованного значения атрибута.
/// </summary>
internal sealed class LeavePolymorphismProjectedAttributeValue
{
    private readonly HashSet<Guid> _valueUuids;

    /// <summary>
    /// Создаёт снимок и отклоняет неразрешённые ссылки и ошибки формул.
    /// </summary>
    public LeavePolymorphismProjectedAttributeValue(ElementAttributeModel attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (attribute.ValueType == null
            || string.IsNullOrEmpty(attribute.ValueTypeReferenceErrorCode) == false
            || string.IsNullOrEmpty(attribute.ValueFormulaErrorCode) == false)
        {
            throw new InvalidOperationException(
                $"Невозможно спроецировать атрибут '{attribute.Name}' [{attribute.LocalUuid}]: "
                + "значение или тип не разрешены.");
        }

        ValueTypeUuid = attribute.ValueType.Uuid;
        IsCollection = attribute.IsCollectionValue;
        _valueUuids = attribute.IsCollectionValue
            ? attribute.Values.Select(x => x.Uuid).ToHashSet()
            : attribute.Value == null ? [] : [attribute.Value.Uuid];
    }

    /// <summary>UUID типа значения.</summary>
    public Guid ValueTypeUuid { get; }

    /// <summary>Признак коллекционного значения.</summary>
    public bool IsCollection { get; }

    /// <summary>Проверяет совместимость определений двух атрибутов.</summary>
    public bool IsCompatibleWith(LeavePolymorphismProjectedAttributeValue other) =>
        ValueTypeUuid == other.ValueTypeUuid && IsCollection == other.IsCollection;

    /// <summary>Сравнивает материализованные значения без учёта порядка коллекции.</summary>
    public bool HasSameValue(LeavePolymorphismProjectedAttributeValue other) =>
        IsCompatibleWith(other) && _valueUuids.SetEquals(other._valueUuids);
}
