using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Управляет поиском и явным созданием значения листа без изменения исходного атрибута.
/// </summary>
public sealed class LeaveValueLookupVM : ViewModelBase
{
    private readonly ILeaveAttributeValueService _attributeValueService;
    private readonly IRelayCommand _createCommand;
    private string? _systemValue;
    private IReadOnlyList<LeaveAttributeValueDraft> _attributeValues = [];
    private bool _hasAttributeValues;
    private LeaveAttributeMatchResult _result =
        new(LeaveAttributeMatchStatus.Invalid, []);
    private TreeLeaveModel? _createdLeave;

    /// <summary>
    /// Инициализирует редактор поиска для указанного типа значения.
    /// </summary>
    /// <param name="valueType">Узел, среди прямых листьев которого выполняется поиск.</param>
    /// <param name="attributeValueService">Сервис поиска и создания значений.</param>
    /// <param name="commandFactory">Фабрика команды явного создания.</param>
    public LeaveValueLookupVM(
        TreeNodeModel valueType,
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory)
    {
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        _attributeValueService = attributeValueService
            ?? throw new ArgumentNullException(nameof(attributeValueService));
        ArgumentNullException.ThrowIfNull(commandFactory);

        _createCommand = commandFactory.Create(_ => Create(), _ => CanCreate);
        if (IsSystemValue)
        {
            Refresh();
        }
        else
        {
            AttributeCriteria = ValueType.Attributes
                .Where(x => x.IsRuntime == false)
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.DeclaringUuid)
                .Select(x => new LeaveValueLookupCriterionVM(
                    x, RefreshAttributeCriteria, commandFactory))
                .ToArray();
            RefreshAttributeCriteria();
        }
    }

    /// <summary>
    /// Узел типа искомого или создаваемого значения.
    /// </summary>
    public TreeNodeModel ValueType { get; }

    /// <summary>
    /// Указывает, что редактор работает с системным типом значения.
    /// </summary>
    public bool IsSystemValue => ValueType is SystemBaseTreeNodeModel;

    /// <summary>
    /// Строковое представление критерия поиска системного значения.
    /// </summary>
    public string? SystemValue
    {
        get => _systemValue;
        set
        {
            if (SetProperty(ref _systemValue, value))
                Refresh();
        }
    }

    /// <summary>
    /// Независимые критерии поиска пользовательского значения.
    /// </summary>
    public IReadOnlyList<LeaveAttributeValueDraft> AttributeValues => _attributeValues;

    /// <summary>
    /// Полный набор редактируемых нерuntime-критериев пользовательского значения.
    /// </summary>
    public IReadOnlyList<LeaveValueLookupCriterionVM> AttributeCriteria { get; } = [];

    /// <summary>
    /// Текущий статус поиска.
    /// </summary>
    public LeaveAttributeMatchStatus Status => _result.Status;

    /// <summary>
    /// Найденные листья.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> Matches => _result.Matches;

    /// <summary>
    /// Количество найденных листьев.
    /// </summary>
    public int MatchCount => Matches.Count;

    /// <summary>
    /// Локализованное описание текущего результата поиска.
    /// </summary>
    public string StatusText => Status switch
    {
        LeaveAttributeMatchStatus.Invalid => "Условия поиска не заполнены или невалидны",
        LeaveAttributeMatchStatus.NotFound => "Подходящие значения не найдены",
        LeaveAttributeMatchStatus.Resolved => "Найдено одно подходящее значение",
        LeaveAttributeMatchStatus.Ambiguous => "Найдено несколько подходящих значений",
        _ => string.Empty,
    };

    /// <summary>
    /// Единственный найденный лист при однозначном результате.
    /// </summary>
    public TreeLeaveModel? ResolvedMatch => _result.ResolvedMatch;

    /// <summary>
    /// Последний лист, созданный явной командой редактора.
    /// </summary>
    public TreeLeaveModel? CreatedLeave
    {
        get => _createdLeave;
        private set => SetProperty(ref _createdLeave, value);
    }

    /// <summary>
    /// Указывает, можно ли создать значение для текущих критериев.
    /// </summary>
    public bool CanCreate =>
        Status == LeaveAttributeMatchStatus.NotFound
        && (ValueType is not SystemBaseTreeNodeModel systemType
            || systemType.SystemBaseType != SystemBaseType.BOOL);

    /// <summary>
    /// Команда явного создания отсутствующего значения.
    /// </summary>
    public IRelayCommand CreateCommand => _createCommand;

    /// <summary>
    /// Заменяет критерии пользовательского значения и сразу повторяет поиск.
    /// </summary>
    /// <param name="values">Полный набор нерuntime-атрибутов.</param>
    public void SetAttributeValues(IEnumerable<LeaveAttributeValueDraft> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _attributeValues = values.ToArray();
        _hasAttributeValues = true;
        OnPropertyChanged(nameof(AttributeValues));
        Refresh();
    }

    /// <summary>
    /// Повторяет поиск по текущим критериям и прямым активным листьям.
    /// </summary>
    public void Refresh()
    {
        _result = ValueType switch
        {
            SystemBaseTreeNodeModel systemType =>
                _attributeValueService.FindSystemValue(systemType, SystemValue),
            _ when _hasAttributeValues && AttributeCriteria.All(x => x.IsValid) =>
                _attributeValueService.FindMatches(
                    AttributeValues,
                    ValueType.ChildLeaves.Where(IsActive)),
            _ => new(LeaveAttributeMatchStatus.Invalid, []),
        };

        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Matches));
        OnPropertyChanged(nameof(MatchCount));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ResolvedMatch));
        OnPropertyChanged(nameof(CanCreate));
        _createCommand.RaiseCanExecuteChanged();
    }

    private void RefreshAttributeCriteria() =>
        SetAttributeValues(AttributeCriteria.Select(x => x.CreateDraft()));

    private void Create()
    {
        if (CanCreate == false)
            return;

        CreatedLeave = ValueType is SystemBaseTreeNodeModel systemType
            ? _attributeValueService.CreateSystemValue(systemType, SystemValue!)
            : _attributeValueService.CreateLeave(ValueType, AttributeValues);
        Refresh();
    }

    private static bool IsActive(TreeLeaveModel leave) =>
        leave.AuditInfo.IsDeleted == false
        && leave.State is not (State.ForSoftDelete or State.ForHardDelete or State.SoftDeleted);
}
