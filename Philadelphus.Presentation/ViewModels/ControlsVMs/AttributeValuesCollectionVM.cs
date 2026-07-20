using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Хранит презентационное состояние выбора листов для одного зафиксированного коллекционного атрибута.
/// </summary>
public sealed class AttributeValuesCollectionVM : ViewModelBase
{
    private readonly ElementAttributeModel _attribute;

    /// <summary>
    /// Инициализирует редактор значений коллекционного атрибута.
    /// </summary>
    /// <param name="attribute">Доменная модель редактируемого атрибута.</param>
    /// <exception cref="ArgumentException">
    /// Атрибут не является коллекционным.
    /// </exception>
    public AttributeValuesCollectionVM(ElementAttributeModel attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        if (attribute.IsCollectionValue == false)
            throw new ArgumentException("Редактор значений доступен только для коллекционного атрибута.", nameof(attribute));

        _attribute = attribute;
        Refresh();
    }

    /// <summary>
    /// Атрибут, с которым было открыто окно. Смена выделения его не заменяет.
    /// </summary>
    public ElementAttributeModel Attribute => _attribute;

    /// <summary>
    /// Активные листья прямого типа данных атрибута.
    /// </summary>
    public IReadOnlyList<AttributeValueSelectionItemVM> Values { get; private set; } = [];

    /// <summary>
    /// Указывает, можно ли изменять эффективную коллекцию значений.
    /// </summary>
    public bool CanSelectValues =>
        _attribute.InheritedAttributeFromParent?.Override != OverrideType.Sealed;

    /// <summary>
    /// Поясняет, почему выбор значений недоступен.
    /// </summary>
    public string SelectionToolTip => CanSelectValues
        ? string.Empty
        : "Коллекция запечатана родительским атрибутом и не может быть переопределена.";

    /// <summary>
    /// Перестраивает список из текущих активных листьев типа данных.
    /// </summary>
    public void Refresh()
    {
        Values = _attribute.ValueType?.ChildLeaves
            .Where(IsActive)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Uuid)
            .Select(x => new AttributeValueSelectionItemVM(this, x))
            .ToArray()
            ?? [];

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(CanSelectValues));
        OnPropertyChanged(nameof(SelectionToolTip));
    }

    internal bool IsSelected(TreeLeaveModel value) =>
        _attribute.Values.Any(x => x.Uuid == value.Uuid);

    internal bool TrySetSelected(TreeLeaveModel value, bool selected)
    {
        if (CanSelectValues == false || selected == IsSelected(value))
            return false;

        return selected
            ? _attribute.TryAddValueToValuesCollection(value)
            : _attribute.TryRemoveValueFromValuesCollection(value);
    }

    private static bool IsActive(TreeLeaveModel value) =>
        value.AuditInfo.IsDeleted == false
        && value.State is not State.ForSoftDelete
            and not State.ForHardDelete
            and not State.SoftDeleted;
}

/// <summary>
/// Хранит презентационное состояние строки выбора одного допустимого значения массива.
/// </summary>
public sealed class AttributeValueSelectionItemVM : ViewModelBase
{
    private readonly AttributeValuesCollectionVM _owner;

    /// <summary>
    /// Инициализирует строку выбора значения массива.
    /// </summary>
    /// <param name="owner">Редактор коллекционного атрибута.</param>
    /// <param name="value">Доменная модель доступного листа.</param>
    internal AttributeValueSelectionItemVM(
        AttributeValuesCollectionVM owner,
        TreeLeaveModel value)
    {
        _owner = owner;
        Value = value;
    }

    /// <summary>
    /// Доступный лист коллекционного атрибута.
    /// </summary>
    public TreeLeaveModel Value { get; }

    /// <summary>
    /// Указывает, входит ли лист в эффективную коллекцию значений.
    /// </summary>
    public bool IsSelected
    {
        get => _owner.IsSelected(Value);
        set
        {
            if (_owner.TrySetSelected(Value, value))
                OnPropertyChanged(nameof(IsSelected));
        }
    }

    /// <summary>
    /// Указывает, можно ли изменить выбор листа.
    /// </summary>
    public bool IsEnabled => _owner.CanSelectValues;

    /// <summary>
    /// Поясняет, почему выбор листа недоступен.
    /// </summary>
    public string ToolTip => _owner.SelectionToolTip;
}
