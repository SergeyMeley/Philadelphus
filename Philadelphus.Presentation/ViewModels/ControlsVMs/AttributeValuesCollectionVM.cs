using System.ComponentModel;
using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Models.Tables;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Services.Tables;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Хранит презентационное состояние выбора листов для одного зафиксированного коллекционного атрибута.
/// </summary>
public sealed class AttributeValuesCollectionVM : ViewModelBase
{
    private readonly ElementAttributeModel _attribute;
    private readonly ILeaveAttributeValueService? _attributeValueService;
    private readonly IAttributeValueCreationConfirmationService? _creationConfirmationService;
    private ElementAttributeVM? _attributeVM;
    private string? _systemSearchValue;
    private LeaveAttributeMatchResult _searchResult =
        new(LeaveAttributeMatchStatus.Invalid, []);

    /// <summary>
    /// Инициализирует редактор значений коллекционного атрибута.
    /// </summary>
    /// <param name="attribute">Доменная модель редактируемого атрибута.</param>
    /// <exception cref="ArgumentException">
    /// Атрибут не является коллекционным.
    /// </exception>
    public AttributeValuesCollectionVM(ElementAttributeModel attribute)
        : this(attribute, null)
    {
    }

    /// <summary>
    /// Инициализирует редактор с сервисом поиска значений листьев.
    /// </summary>
    /// <param name="attribute">Доменная модель редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeModel attribute,
        ILeaveAttributeValueService? attributeValueService)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        if (attribute.IsCollectionValue == false)
            throw new ArgumentException("Редактор значений доступен только для коллекционного атрибута.", nameof(attribute));

        _attribute = attribute;
        _attributeValueService = attributeValueService;
        Refresh();
    }

    /// <summary>
    /// Инициализирует редактор для зафиксированной модели представления атрибута.
    /// </summary>
    /// <param name="attribute">Модель представления редактируемого атрибута.</param>
    public AttributeValuesCollectionVM(ElementAttributeVM attribute)
        : this(attribute?.Model ?? throw new ArgumentNullException(nameof(attribute)))
    {
        _attributeVM = attribute;
    }

    /// <summary>
    /// Инициализирует редактор для атрибута и подключает сервис поиска.
    /// </summary>
    /// <param name="attribute">Модель представления редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeVM attribute,
        ILeaveAttributeValueService attributeValueService)
        : this(
            attribute?.Model ?? throw new ArgumentNullException(nameof(attribute)),
            attributeValueService ?? throw new ArgumentNullException(nameof(attributeValueService)))
    {
        _attributeVM = attribute;
    }

    /// <summary>
    /// Инициализирует редактор атрибута с общим редактором поиска и создания листа.
    /// </summary>
    /// <param name="attribute">Доменная модель редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    /// <param name="commandFactory">Фабрика команды явного создания значения.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeModel attribute,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory)
        : this(attribute, attributeValueService)
    {
        ArgumentNullException.ThrowIfNull(commandFactory);
        if (_attribute.ValueType == null)
            return;

        ValueLookup = new LeaveValueLookupVM(
            _attribute.ValueType,
            attributeValueService,
            commandFactory);
        ValueLookup.PropertyChanged += HandleValueLookupPropertyChanged;
    }

    /// <summary>
    /// Инициализирует редактор с подтверждением включения созданного значения в массив.
    /// </summary>
    /// <param name="attribute">Доменная модель редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    /// <param name="commandFactory">Фабрика команды явного создания значения.</param>
    /// <param name="creationConfirmationService">Подтверждение добавления созданного значения.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeModel attribute,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory,
        IAttributeValueCreationConfirmationService creationConfirmationService)
        : this(attribute, attributeValueService, commandFactory)
    {
        _creationConfirmationService = creationConfirmationService
            ?? throw new ArgumentNullException(nameof(creationConfirmationService));
    }

    /// <summary>
    /// Инициализирует редактор для зафиксированной модели представления атрибута.
    /// </summary>
    /// <param name="attribute">Модель представления редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    /// <param name="commandFactory">Фабрика команды явного создания значения.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeVM attribute,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory)
        : this(
            attribute?.Model ?? throw new ArgumentNullException(nameof(attribute)),
            attributeValueService,
            commandFactory)
    {
        _attributeVM = attribute;
    }

    /// <summary>
    /// Инициализирует редактор модели представления с подтверждением созданного значения.
    /// </summary>
    /// <param name="attribute">Модель представления редактируемого атрибута.</param>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    /// <param name="commandFactory">Фабрика команды явного создания значения.</param>
    /// <param name="creationConfirmationService">Подтверждение добавления созданного значения.</param>
    public AttributeValuesCollectionVM(
        ElementAttributeVM attribute,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory,
        IAttributeValueCreationConfirmationService creationConfirmationService)
        : this(
            attribute?.Model ?? throw new ArgumentNullException(nameof(attribute)),
            attributeValueService,
            commandFactory,
            creationConfirmationService)
    {
        _attributeVM = attribute;
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
    /// Дескрипторы колонок таблицы допустимых значений.
    /// </summary>
    public IReadOnlyList<ChildCollectionTableColumn> Columns { get; private set; } = [];

    /// <summary>
    /// Строки таблицы допустимых значений.
    /// </summary>
    public IReadOnlyList<ChildCollectionTableRow> Rows { get; private set; } = [];

    /// <summary>
    /// Строковое значение для автоматического поиска по системному типу.
    /// </summary>
    public string? SystemSearchValue
    {
        get => _systemSearchValue;
        set
        {
            if (SetProperty(ref _systemSearchValue, value))
                RefreshSystemSearch();
        }
    }

    /// <summary>
    /// Указывает, доступен ли поиск системного значения.
    /// </summary>
    public bool CanSearchSystemValues =>
        _attributeValueService != null
        && _attribute.ValueType is SystemBaseTreeNodeModel;

    /// <summary>
    /// Текущий статус поиска системного значения.
    /// </summary>
    public LeaveAttributeMatchStatus SearchStatus => _searchResult.Status;

    /// <summary>
    /// Количество найденных значений.
    /// </summary>
    public int SearchMatchCount => _searchResult.Matches.Count;

    /// <summary>
    /// Однозначно найденный лист.
    /// </summary>
    public TreeLeaveModel? ResolvedSearchMatch => _searchResult.ResolvedMatch;

    /// <summary>
    /// Строка таблицы однозначно найденного листа.
    /// </summary>
    public ChildCollectionTableRow? ResolvedSearchRow { get; private set; }

    /// <summary>
    /// Общий редактор поиска и явного создания значения атрибута.
    /// </summary>
    public LeaveValueLookupVM? ValueLookup { get; }

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
        var leaves = _attribute.ValueType?.ChildLeaves
            .Where(IsActive)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Uuid)
            .ToArray()
            ?? [];
        Values = leaves
            .Select(x => new AttributeValueSelectionItemVM(this, x))
            .ToArray();
        UpdateSearchResult();

        Columns =
        [
            CreateSelectionColumn(),
            CreateSearchMatchColumn(),
            .. LeaveTableProjectionBuilder.buildLeaveTableColumns(leaves, startOrder: 2),
        ];
        RebuildRows(leaves);

        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(Columns));
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(CanSelectValues));
        OnPropertyChanged(nameof(SelectionToolTip));
        NotifySearchProperties();
    }

    internal bool IsSelected(TreeLeaveModel value) =>
        _attribute.Values.Any(x => x.Uuid == value.Uuid);

    internal bool TrySetSelected(TreeLeaveModel value, bool selected)
    {
        if (CanSelectValues == false || selected == IsSelected(value))
            return false;

        var changed = selected
            ? _attribute.TryAddValueToValuesCollection(value)
            : _attribute.TryRemoveValueFromValuesCollection(value);
        if (changed)
            _attributeVM?.RefreshAssignedValues();

        return changed;
    }

    private ChildCollectionTableColumn CreateSelectionColumn() => new(
        "IsSelected",
        "В массиве",
        0,
        child => child is TreeLeaveModel leave && IsSelected(leave),
        isReadOnly: false,
        setterFactory: child => child is TreeLeaveModel leave
            ? value =>
            {
                TrySetSelected(leave, value is true);
                return IsSelected(leave);
            }
            : null,
        columnType: ChildCollectionTableColumnType.CheckBox,
        cellEnabledGetter: _ => CanSelectValues,
        cellToolTipGetter: _ => SelectionToolTip);

    private ChildCollectionTableColumn CreateSearchMatchColumn() => new(
        "IsSearchMatch",
        "Подходит",
        1,
        child => child is TreeLeaveModel leave ? IsSearchMatch(leave) : null,
        columnType: ChildCollectionTableColumnType.CheckBox,
        cellEnabledGetter: _ => false,
        headerToolTip: "Соответствие текущим условиям поиска.");

    private bool? IsSearchMatch(TreeLeaveModel leave) =>
        SearchStatus == LeaveAttributeMatchStatus.Invalid
            ? null
            : _searchResult.Matches.Any(x => x.Uuid == leave.Uuid);

    private void RefreshSystemSearch()
    {
        UpdateSearchResult();
        RebuildRows(Values.Select(x => x.Value));
        OnPropertyChanged(nameof(Rows));
        NotifySearchProperties();
    }

    private void UpdateSearchResult()
    {
        _searchResult = CanSearchSystemValues
            ? _attributeValueService!.FindSystemValue(
                (SystemBaseTreeNodeModel)_attribute.ValueType!,
                _systemSearchValue)
            : new(LeaveAttributeMatchStatus.Invalid, []);
    }

    private void RebuildRows(IEnumerable<TreeLeaveModel> leaves)
    {
        Rows = LeaveTableProjectionBuilder.buildLeaveTableRows(leaves, Columns);
        ResolvedSearchRow = ResolvedSearchMatch == null
            ? null
            : Rows.SingleOrDefault(x => x.SourceUuid == ResolvedSearchMatch.Uuid);
    }

    private void NotifySearchProperties()
    {
        OnPropertyChanged(nameof(CanSearchSystemValues));
        OnPropertyChanged(nameof(SearchStatus));
        OnPropertyChanged(nameof(SearchMatchCount));
        OnPropertyChanged(nameof(ResolvedSearchMatch));
        OnPropertyChanged(nameof(ResolvedSearchRow));
    }

    private void HandleValueLookupPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (ValueLookup == null)
            return;

        if (eventArgs.PropertyName == nameof(LeaveValueLookupVM.SystemValue))
        {
            _systemSearchValue = ValueLookup.SystemValue;
            OnPropertyChanged(nameof(SystemSearchValue));
        }
        else if (eventArgs.PropertyName == nameof(LeaveValueLookupVM.CreatedLeave))
        {
            Refresh();
            if (_creationConfirmationService != null
                && ValueLookup.CreatedLeave is { } createdLeave)
            {
                _ = ConfirmCreatedValueAdditionAsync(createdLeave);
            }
        }
        else if (eventArgs.PropertyName == nameof(LeaveValueLookupVM.Status))
        {
            _searchResult = new(ValueLookup.Status, ValueLookup.Matches);
            RebuildRows(Values.Select(x => x.Value));
            OnPropertyChanged(nameof(Rows));
            NotifySearchProperties();
        }
    }

    private async Task ConfirmCreatedValueAdditionAsync(TreeLeaveModel createdLeave)
    {
        if (CanSelectValues == false
            || await _creationConfirmationService!.ConfirmAdditionAsync(
                createdLeave,
                _attribute) == false)
        {
            return;
        }

        if (TrySetSelected(createdLeave, selected: true))
            Refresh();
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
