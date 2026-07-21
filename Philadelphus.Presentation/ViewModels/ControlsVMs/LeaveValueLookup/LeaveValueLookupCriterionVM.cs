using System.Collections.ObjectModel;
using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Представляет один редактируемый критерий поиска пользовательского листа.
/// </summary>
public sealed class LeaveValueLookupCriterionVM : ViewModelBase
{
    private readonly Action _changed;
    private readonly ILeaveAttributeValueService _attributeValueService;
    private readonly IRelayCommandFactory _commandFactory;
    private readonly ObservableCollection<LeaveValueLookupOptionVM> _availableValues;
    private IRelayCommand? _clearCommand;
    private LeaveValueLookupOptionVM? _selectedValue;
    private string _valueText = string.Empty;
    private bool _isValueInputValid = true;

    /// <summary>
    /// Инициализирует критерий по эффективному объявлению атрибута.
    /// </summary>
    /// <param name="attribute">Атрибут типа искомого листа.</param>
    /// <param name="changed">Обработчик изменения критерия.</param>
    /// <param name="attributeValueService">Сервис поиска и создания системных значений.</param>
    /// <param name="commandFactory">Фабрика команд редактора критерия.</param>
    public LeaveValueLookupCriterionVM(
        ElementAttributeModel attribute,
        Action changed,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(changed);
        ArgumentNullException.ThrowIfNull(attributeValueService);
        ArgumentNullException.ThrowIfNull(commandFactory);

        Attribute = attribute;
        _changed = changed;
        _attributeValueService = attributeValueService;
        _commandFactory = commandFactory;
        _availableValues = new(attribute.ValueType?.ChildLeaves
            .Where(IsActive)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Uuid)
            .Select(x => new LeaveValueLookupOptionVM(this, x))
            ?? []);
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
    /// Указывает, что одиночное значение системного базового типа можно ввести вручную.
    /// </summary>
    public bool CanEnterValueManually =>
        IsCollection == false && Attribute.ValueType is SystemBaseTreeNodeModel;

    /// <summary>
    /// Указывает, что одиночное значение выбирается только из выпадающего списка.
    /// </summary>
    public bool CanSelectValueFromList => IsCollection == false && CanEnterValueManually == false;

    /// <summary>
    /// Указывает, что тип значений критерия разрешён.
    /// </summary>
    public bool IsValid =>
        Attribute.ValueType != null
        && string.IsNullOrEmpty(Attribute.ValueTypeReferenceErrorCode)
        && _isValueInputValid;

    /// <summary>
    /// Активные значения прямого типа атрибута.
    /// </summary>
    public IReadOnlyList<LeaveValueLookupOptionVM> AvailableValues => _availableValues;

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
            var changed = SetSelectedValue(value, synchronizeText: true);
            changed |= SetValueInputValid(true);
            if (changed)
                _changed();
        }
    }

    /// <summary>
    /// Текст системного базового значения для ручного ввода в редактируемом списке.
    /// </summary>
    public string ValueText
    {
        get => _valueText;
        set
        {
            if (SetProperty(ref _valueText, value ?? string.Empty) == false)
                return;

            ApplyValueText();
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

        if (CanEnterValueManually)
            ValueText = string.Empty;
        else
            SelectedValue = null;
    }

    private void ApplyValueText()
    {
        if (CanEnterValueManually == false)
            return;

        var normalizedText = ValueText.Trim();
        if (normalizedText.Length == 0)
        {
            var changed = SetSelectedValue(null, synchronizeText: false);
            changed |= SetValueInputValid(true);
            if (changed)
                _changed();
            return;
        }

        var valueType = (SystemBaseTreeNodeModel)Attribute.ValueType!;
        var result = _attributeValueService.FindSystemValue(valueType, normalizedText);
        var resolvedValue = result.Matches
            .OfType<SystemBaseTreeLeaveModel>()
            .FirstOrDefault(x => string.Equals(
                x.StringValue,
                normalizedText,
                StringComparison.Ordinal));
        resolvedValue ??= result.ResolvedMatch as SystemBaseTreeLeaveModel;

        if (result.Status == LeaveAttributeMatchStatus.NotFound
            && valueType.SystemBaseType != SystemBaseType.BOOL)
        {
            resolvedValue = _attributeValueService.CreateSystemValue(valueType, normalizedText);
        }

        var isValid = resolvedValue != null;
        var valueChanged = SetSelectedValue(
            isValid ? GetOrAddOption(resolvedValue!) : null,
            synchronizeText: isValid);
        valueChanged |= SetValueInputValid(isValid);
        if (valueChanged || isValid == false)
            _changed();
    }

    private bool SetSelectedValue(
        LeaveValueLookupOptionVM? value,
        bool synchronizeText)
    {
        if (value != null && AvailableValues.Contains(value) == false)
            throw new ArgumentException("Значение отсутствует среди вариантов критерия.", nameof(value));

        var changed = SetProperty(ref _selectedValue, value, nameof(SelectedValue));
        if (synchronizeText && CanEnterValueManually)
        {
            changed |= SetProperty(
                ref _valueText,
                value?.DisplayName ?? string.Empty,
                nameof(ValueText));
        }
        return changed;
    }

    private bool SetValueInputValid(bool value)
    {
        if (_isValueInputValid == value)
            return false;

        _isValueInputValid = value;
        OnPropertyChanged(nameof(IsValid));
        return true;
    }

    private LeaveValueLookupOptionVM GetOrAddOption(TreeLeaveModel value)
    {
        var option = AvailableValues.SingleOrDefault(x => x.Value.Uuid == value.Uuid);
        if (option != null)
            return option;

        option = new LeaveValueLookupOptionVM(this, value);
        _availableValues.Add(option);
        return option;
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
