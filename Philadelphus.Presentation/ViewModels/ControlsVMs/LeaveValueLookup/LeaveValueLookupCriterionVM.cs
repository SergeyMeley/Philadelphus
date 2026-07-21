using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Представляет один редактируемый критерий поиска пользовательского листа.
/// </summary>
public sealed class LeaveValueLookupCriterionVM : ViewModelBase
{
    private readonly Action _changed;
    private readonly IRelayCommandFactory _commandFactory;
    private IRelayCommand? _clearCommand;
    private LeaveValueLookupOptionVM? _selectedValue;

    /// <summary>
    /// Инициализирует критерий по эффективному объявлению атрибута.
    /// </summary>
    /// <param name="attribute">Атрибут типа искомого листа.</param>
    /// <param name="changed">Обработчик изменения критерия.</param>
    /// <param name="commandFactory">Фабрика команд редактора критерия.</param>
    public LeaveValueLookupCriterionVM(
        ElementAttributeModel attribute,
        Action changed, IRelayCommandFactory commandFactory)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(changed);
        ArgumentNullException.ThrowIfNull(commandFactory);

        Attribute = attribute;
        _changed = changed;
        _commandFactory = commandFactory;
        AvailableValues = attribute.ValueType?.ChildLeaves
            .Where(IsActive)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Uuid)
            .Select(x => new LeaveValueLookupOptionVM(this, x))
            .ToArray()
            ?? [];
    }

    /// <summary>
    /// Эффективный атрибут, задающий критерий.
    /// </summary>
    public ElementAttributeModel Attribute { get; }

    /// <summary>
    /// UUID объявления, используемый вместо пользовательского имени.
    /// </summary>
    public Guid DeclaringUuid => Attribute.DeclaringUuid;

    /// <summary>
    /// Пользовательское имя критерия.
    /// </summary>
    public string Name => Attribute.Name;

    /// <summary>
    /// Указывает, что критерий принимает несколько значений.
    /// </summary>
    public bool IsCollection => Attribute.IsCollectionValue;

    /// <summary>
    /// Указывает, что тип значений критерия разрешён.
    /// </summary>
    public bool IsValid =>
        Attribute.ValueType != null
        && string.IsNullOrEmpty(Attribute.ValueTypeReferenceErrorCode);

    /// <summary>
    /// Активные значения прямого типа атрибута.
    /// </summary>
    public IReadOnlyList<LeaveValueLookupOptionVM> AvailableValues { get; }

    /// <summary>
    /// Команда очистки выбранных значений критерия.
    /// </summary>
    public IRelayCommand ClearCommand =>
        _clearCommand ??= _commandFactory.Create(_ => Clear());

    /// <summary>
    /// Выбранное значение скалярного критерия или null для пустого значения.
    /// </summary>
    public LeaveValueLookupOptionVM? SelectedValue
    {
        get => _selectedValue;
        set
        {
            if (value != null && AvailableValues.Contains(value) == false)
                throw new ArgumentException("Значение отсутствует среди вариантов критерия.", nameof(value));
            if (SetProperty(ref _selectedValue, value))
                _changed();
        }
    }

    internal LeaveAttributeValueDraft CreateDraft() => IsCollection
        ? LeaveAttributeValueDraft.Collection(
            DeclaringUuid,
            AvailableValues.Where(x => x.IsSelected).Select(x => x.Value.Uuid))
        : LeaveAttributeValueDraft.Scalar(DeclaringUuid, SelectedValue?.Value.Uuid);

    internal void SetValue(ElementAttributeModel? source)
    {
        if (source != null && source.DeclaringUuid != DeclaringUuid)
            throw new ArgumentException(
                "Значение относится к другому объявлению атрибута.",
                nameof(source));

        if (IsCollection)
        {
            var selectedUuids = source?.Values.Select(x => x.Uuid).ToHashSet() ?? [];
            foreach (var option in AvailableValues)
                option.IsSelected = selectedUuids.Contains(option.Value.Uuid);
        }
        else
        {
            SelectedValue = source?.Value == null
                ? null
                : AvailableValues.SingleOrDefault(x => x.Value.Uuid == source.Value.Uuid);
        }
    }

    internal void NotifyCollectionChanged() => _changed();

    internal void Clear()
    {
        if (IsCollection)
        {
            foreach (var option in AvailableValues.Where(x => x.IsSelected).ToArray())
                option.IsSelected = false;
            return;
        }

        SelectedValue = null;
    }

    private static bool IsActive(TreeLeaveModel leave) =>
        leave.AuditInfo.IsDeleted == false
        && leave.State is not (State.ForSoftDelete or State.ForHardDelete or State.SoftDeleted);
}

/// <summary>
/// Представляет один вариант значения пользовательского критерия.
/// </summary>
public sealed class LeaveValueLookupOptionVM : ViewModelBase
{
    private readonly LeaveValueLookupCriterionVM _owner;
    private bool _isSelected;

    internal LeaveValueLookupOptionVM(
        LeaveValueLookupCriterionVM owner,
        TreeLeaveModel value)
    {
        _owner = owner;
        Value = value;
    }

    /// <summary>
    /// Доменный лист варианта значения.
    /// </summary>
    public TreeLeaveModel Value { get; }

    /// <summary>
    /// Отображаемое системное значение или имя пользовательского листа.
    /// </summary>
    public string DisplayName => Value is SystemBaseTreeLeaveModel systemValue
        ? systemValue.StringValue
        : Value.Name;

    /// <summary>
    /// Указывает, включён ли вариант в коллекционный критерий.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
                _owner.NotifyCollectionChanged();
        }
    }
}
