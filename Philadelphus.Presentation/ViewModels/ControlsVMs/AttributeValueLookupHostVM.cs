using System.ComponentModel;
using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Models.Tables;
using Philadelphus.Presentation.Services.Tables;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Связывает общий поиск листа с явным присвоением значения одиночному атрибуту.
/// </summary>
public sealed class AttributeValueLookupHostVM : ViewModelBase, IDisposable
{
    private bool _showOnlyMatches;
    private bool _isDisposed;

    /// <summary>
    /// Инициализирует хост поиска значения зафиксированного атрибута.
    /// </summary>
    /// <param name="attribute">Редактируемый атрибут.</param>
    /// <param name="attributeValueService">Сервис поиска и создания листьев.</param>
    /// <param name="commandFactory">Фабрика команды явного выбора.</param>
    public AttributeValueLookupHostVM(
        ElementAttributeVM attribute,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(attributeValueService);
        ArgumentNullException.ThrowIfNull(commandFactory);

        Attribute = attribute;
        if (IsAvailableFor(attribute) == false)
            throw new ArgumentException(
                "Расширенный поиск доступен только одиночному пользовательскому атрибуту.",
                nameof(attribute));
        var valueType = attribute.SelectedValueType
            ?? throw new ArgumentException("Тип значения атрибута не задан.", nameof(attribute));

        ValueLookup = new LeaveValueLookupVM(
            valueType,
            attributeValueService,
            commandFactory,
            enablePartialMatch: true);
        ValueLookup.PropertyChanged += HandleLookupPropertyChanged;
        ValueLookup.SetAttributeValuesFrom(Attribute.AssignedValue);
        RebuildTable();
    }

    /// <summary>
    /// Проверяет доступность расширенного поиска для атрибута.
    /// </summary>
    public static bool IsAvailableFor(ElementAttributeVM? attribute) =>
        attribute != null
        && attribute.IsRuntime == false
        && attribute.IsCollectionValue == false
        && attribute.SelectedValueType is not null and not SystemBaseTreeNodeModel
        && string.IsNullOrEmpty(attribute.ValueTypeReferenceErrorCode);

    /// <summary>
    /// Атрибут, значение которого изменяется только явной командой выбора.
    /// </summary>
    public ElementAttributeVM Attribute { get; }

    /// <summary>
    /// Общий редактор поиска и создания листа.
    /// </summary>
    public LeaveValueLookupVM ValueLookup { get; }

    /// <summary>
    /// Кандидаты текущего результата поиска.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> Matches => ValueLookup.Matches;

    /// <summary>
    /// Дескрипторы колонок таблицы доступных значений без колонки включения в массив.
    /// </summary>
    public IReadOnlyList<ChildCollectionTableColumn> Columns { get; private set; } = [];

    /// <summary>
    /// Строки таблицы доступных значений.
    /// </summary>
    public IReadOnlyList<ChildCollectionTableRow> Rows { get; private set; } = [];

    /// <summary>
    /// Количество значений, соответствующих текущим критериям.
    /// </summary>
    public int SearchMatchCount => ValueLookup.MatchCount;

    /// <summary>
    /// Строка единственного найденного значения.
    /// </summary>
    public ChildCollectionTableRow? ResolvedSearchRow { get; private set; }

    /// <summary>
    /// Указывает, что таблица должна скрывать неподходящие значения.
    /// </summary>
    public bool ShowOnlyMatches
    {
        get => _showOnlyMatches;
        set
        {
            if (SetProperty(ref _showOnlyMatches, value))
                RebuildRows();
        }
    }

    /// <summary>
    /// Освобождает подписку на общий редактор поиска.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        ValueLookup.PropertyChanged -= HandleLookupPropertyChanged;
    }

    private void HandleLookupPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (_isDisposed)
            return;

        if (eventArgs.PropertyName == nameof(LeaveValueLookupVM.CreatedLeave)
            && ValueLookup.CreatedLeave is { } createdLeave)
        {
            Attribute.AssignedValue = createdLeave;
        }
        else if (eventArgs.PropertyName == nameof(LeaveValueLookupVM.Matches))
        {
            OnPropertyChanged(nameof(Matches));
            OnPropertyChanged(nameof(SearchMatchCount));
            RebuildRows();
        }
    }

    private void RebuildTable()
    {
        var leaves = GetAvailableLeaves();
        Columns =
        [
            CreateSelectionColumn(),
            CreateSearchMatchColumn(),
            .. LeaveTableProjectionBuilder.buildLeaveTableColumns(leaves, startOrder: 2),
        ];
        OnPropertyChanged(nameof(Columns));
        RebuildRows(leaves);
    }

    private void RebuildRows(IEnumerable<TreeLeaveModel>? leaves = null)
    {
        var values = leaves?.ToArray() ?? GetAvailableLeaves();
        var rows = LeaveTableProjectionBuilder.buildLeaveTableRows(values, Columns);
        var matchingUuids = Matches.Select(x => x.Uuid).ToHashSet();
        Rows = ShowOnlyMatches && ValueLookup.Status != LeaveAttributeMatchStatus.Invalid
            ? rows.Where(x => matchingUuids.Contains(x.SourceUuid)).ToArray()
            : rows;
        ResolvedSearchRow = ValueLookup.ResolvedMatch == null
            ? null
            : Rows.SingleOrDefault(x => x.SourceUuid == ValueLookup.ResolvedMatch.Uuid);
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(ResolvedSearchRow));
    }

    private ChildCollectionTableColumn CreateSearchMatchColumn() => new(
        "IsSearchMatch",
        "Подходит",
        1,
        child => child is TreeLeaveModel leave
            ? IsSearchMatch(leave)
            : null,
        columnType: ChildCollectionTableColumnType.CheckBox,
        cellEnabledGetter: _ => false,
        headerToolTip: "Соответствие текущим условиям поиска.");

    private ChildCollectionTableColumn CreateSelectionColumn() => new(
        "IsSelected",
        "Выбрано",
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
        headerToolTip: "Для одиночного атрибута может быть выбрано только одно значение.");

    private bool IsSelected(TreeLeaveModel leave) =>
        Attribute.AssignedValue?.Uuid == leave.Uuid;

    private void TrySetSelected(TreeLeaveModel leave, bool selected)
    {
        if (selected == IsSelected(leave))
            return;

        Attribute.AssignedValue = selected ? leave : null;
        RebuildRows();
    }

    private bool? IsSearchMatch(TreeLeaveModel leave) =>
        ValueLookup.Status == LeaveAttributeMatchStatus.Invalid
            ? null
            : Matches.Any(x => x.Uuid == leave.Uuid);

    private TreeLeaveModel[] GetAvailableLeaves() =>
        ValueLookup.ValueType.ChildLeaves
            .Where(IsActive)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Uuid)
            .ToArray();

    private static bool IsActive(TreeLeaveModel value) =>
        value.AuditInfo.IsDeleted == false
        && value.State is not State.ForSoftDelete
            and not State.ForHardDelete
            and not State.SoftDeleted;
}
