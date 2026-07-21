using System.ComponentModel;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Связывает общий поиск листа с явным присвоением значения одиночному атрибуту.
/// </summary>
public sealed class AttributeValueLookupHostVM : ViewModelBase, IDisposable
{
    private readonly IRelayCommand _selectCommand;
    private TreeLeaveModel? _selectedMatch;
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
        Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        ArgumentNullException.ThrowIfNull(attributeValueService);
        ArgumentNullException.ThrowIfNull(commandFactory);
        if (IsAvailableFor(attribute) == false)
            throw new ArgumentException(
                "Расширенный поиск доступен только одиночному пользовательскому атрибуту.",
                nameof(attribute));
        var valueType = attribute.SelectedValueType
            ?? throw new ArgumentException("Тип значения атрибута не задан.", nameof(attribute));

        ValueLookup = new LeaveValueLookupVM(
            valueType,
            attributeValueService,
            commandFactory);
        _selectCommand = commandFactory.Create(_ => Select(), _ => CanSelect);
        ValueLookup.PropertyChanged += HandleLookupPropertyChanged;
        ValueLookup.SetAttributeValuesFrom(Attribute.AssignedValue);
        SynchronizeSelectedMatch();
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
    /// Кандидат, выбранный пользователем или однозначно разрешённый поиском.
    /// </summary>
    public TreeLeaveModel? SelectedMatch
    {
        get => _selectedMatch;
        set
        {
            var match = value == null
                ? null
                : Matches.SingleOrDefault(x => x.Uuid == value.Uuid)
                    ?? throw new ArgumentException(
                        "Лист отсутствует среди текущих результатов поиска.",
                        nameof(value));
            if (SetProperty(ref _selectedMatch, match))
            {
                OnPropertyChanged(nameof(CanSelect));
                _selectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Указывает, можно ли явно присвоить выбранного кандидата.
    /// </summary>
    public bool CanSelect => _isDisposed == false && SelectedMatch != null;

    /// <summary>
    /// Явно записывает выбранный лист формулой-ссылкой в исходный атрибут.
    /// </summary>
    public IRelayCommand SelectCommand => _selectCommand;

    /// <summary>
    /// Освобождает подписку на общий редактор поиска.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        ValueLookup.PropertyChanged -= HandleLookupPropertyChanged;
        OnPropertyChanged(nameof(CanSelect));
        _selectCommand.RaiseCanExecuteChanged();
    }

    private void Select()
    {
        if (CanSelect && SelectedMatch is { } selectedMatch)
            Attribute.AssignedValue = selectedMatch;
    }

    private void HandleLookupPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (_isDisposed || eventArgs.PropertyName != nameof(LeaveValueLookupVM.Status))
            return;

        OnPropertyChanged(nameof(Matches));
        SynchronizeSelectedMatch();
    }

    private void SynchronizeSelectedMatch()
    {
        SelectedMatch = ValueLookup.ResolvedMatch
            ?? Matches.SingleOrDefault(x => x.Uuid == SelectedMatch?.Uuid);
    }
}
