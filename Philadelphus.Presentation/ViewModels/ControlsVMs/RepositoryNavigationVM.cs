using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Serilog;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Управляет переходами к элементам и атрибутам открытого репозитория.
/// </summary>
public sealed class RepositoryNavigationVM : ControlBaseVM
{
    private readonly RepositoryExplorerControlVM _repositoryExplorerVM;
    private readonly IRelayCommandFactory _commandFactory;
    private readonly Stack<NavigationLocation> _backHistory = new();
    private readonly Stack<NavigationLocation> _forwardHistory = new();
    private NavigationLocation? _currentLocation;
    private bool _isNavigationInProgress;
    private Guid? _targetUuid;
    private IRelayCommand? _navigateBackCommand;
    private IRelayCommand? _navigateForwardCommand;
    private IRelayCommand? _navigateToAttributeDataTypeCommand;
    private IRelayCommand? _navigateToAttributeValueCommand;
    private IRelayCommand? _navigateToLeavesOwnerCommand;

    /// <summary>
    /// Инициализирует модель представления навигации по репозиторию.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
    /// <param name="mapper">Экземпляр AutoMapper.</param>
    /// <param name="logger">Сервис журналирования.</param>
    /// <param name="notificationService">Сервис уведомлений.</param>
    /// <param name="applicationCommandsVM">Модель команд приложения.</param>
    /// <param name="repositoryExplorerVM">Родительская модель обозревателя репозитория.</param>
    /// <param name="commandFactory">Фабрика синхронных команд.</param>
    public RepositoryNavigationVM(
        IServiceProvider serviceProvider, 
        IMapper mapper,
        ILogger logger,
        INotificationService notificationService, 
        IApplicationCommandsVM applicationCommandsVM,
        RepositoryExplorerControlVM repositoryExplorerVM,
        IRelayCommandFactory commandFactory)
        : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
    {
        ArgumentNullException.ThrowIfNull(repositoryExplorerVM);
        ArgumentNullException.ThrowIfNull(commandFactory);

        _repositoryExplorerVM = repositoryExplorerVM;
        _commandFactory = commandFactory;
    }

    /// <summary>
    /// Команда перехода к предыдущей позиции в истории навигации.
    /// </summary>
    public IRelayCommand NavigateBackCommand =>
        _navigateBackCommand ??= _commandFactory.Create(
            obj =>
            {
                NavigateThroughHistory(_backHistory, _forwardHistory);
            },
            ce =>
            {
                return _backHistory.Count > 0;
            });

    /// <summary>
    /// Команда перехода к следующей позиции в истории навигации.
    /// </summary>
    public IRelayCommand NavigateForwardCommand =>
        _navigateForwardCommand ??= _commandFactory.Create(
            obj =>
            {
                NavigateThroughHistory(_forwardHistory, _backHistory);
            },
            ce =>
            {
                return _forwardHistory.Count > 0;
            });

    /// <summary>
    /// Команда перехода к узлу, выбранному в качестве типа данных атрибута.
    /// </summary>
    public IRelayCommand NavigateToAttributeDataTypeCommand =>
        _navigateToAttributeDataTypeCommand ??= _commandFactory.Create(
            parameter =>
            {
                if (parameter is ElementAttributeVM { SelectedValueType: { } valueType })
                    Navigate(valueType.Uuid);
            },
            parameter => parameter is ElementAttributeVM { SelectedValueType: not null });

    /// <summary>
    /// Команда перехода к листу, выбранному в качестве одиночного значения атрибута.
    /// </summary>
    public IRelayCommand NavigateToAttributeValueCommand =>
        _navigateToAttributeValueCommand ??= _commandFactory.Create(
            parameter =>
            {
                if (parameter is ElementAttributeVM { IsCollectionValue: false, AssignedValue: { } value })
                    Navigate(value.Uuid);
            },
            parameter => parameter is ElementAttributeVM
            {
                IsCollectionValue: false,
                AssignedValue: not null
            });

    /// <summary>
    /// Команда перехода к родительскому узлу отображаемой таблицы листов.
    /// </summary>
    public IRelayCommand NavigateToLeavesOwnerCommand =>
        _navigateToLeavesOwnerCommand ??= _commandFactory.Create(
            obj =>
            {
                if (_repositoryExplorerVM.CurrentLeavesOwner is IMainEntityVM<IMainEntityModel> owner)
                    Navigate(owner.Uuid);
            },
            ce =>
            {
                return _repositoryExplorerVM.CurrentLeavesOwner is IMainEntityVM<IMainEntityModel>;
            });

    /// <summary>
    /// Обновляет доступность команд навигации, зависящих от текущего элемента обозревателя.
    /// </summary>
    internal void NotifyCurrentElementChanged()
    {
        _navigateToLeavesOwnerCommand?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Регистрирует выбранный элемент репозитория как текущую позицию навигации.
    /// </summary>
    /// <param name="repositoryMember">Выбранный элемент репозитория.</param>
    internal void NotifySelectedRepositoryMemberChanged(
        IMainEntityVM<IMainEntityModel>? repositoryMember)
    {
        if (_isNavigationInProgress || repositoryMember == null)
            return;

        RegisterLocation(new NavigationLocation(repositoryMember.Uuid, null));
    }

    /// <summary>
    /// Регистрирует выбранный атрибут либо его владельца как текущую позицию навигации.
    /// </summary>
    /// <param name="attribute">Выбранный атрибут либо null.</param>
    internal void NotifySelectedAttributeChanged(ElementAttributeVM? attribute)
    {
        if (_isNavigationInProgress
            || _repositoryExplorerVM.SelectedRepositoryMember is not { } owner)
            return;

        var location = attribute == null
            ? new NavigationLocation(owner.Uuid, null)
            : new NavigationLocation(attribute.Uuid, owner.Uuid);
        RegisterLocation(location);
    }

    /// <summary>
    /// Очищает историю переходов и сохраняет текущий выбранный элемент как начальную позицию.
    /// </summary>
    internal void ResetHistory()
    {
        _backHistory.Clear();
        _forwardHistory.Clear();
        _currentLocation = GetCurrentLocation();
        RaiseHistoryCanExecuteChanged();
    }

    /// <summary>
    /// UUID элемента, к которому был запрошен последний переход.
    /// </summary>
    public Guid? TargetUuid
    {
        get => _targetUuid;
        private set => SetProperty(ref _targetUuid, value);
    }

    /// <summary>
    /// Выполняет переход к элементу либо к атрибуту его владельца.
    /// </summary>
    /// <param name="targetUuid">UUID целевого элемента или атрибута.</param>
    /// <param name="ownerUuid">UUID владельца атрибута либо null для перехода к элементу.</param>
    /// <returns>true, если целевой элемент найден и переход инициирован; иначе false.</returns>
    public bool Navigate(Guid targetUuid, Guid? ownerUuid = null)
    {
        _currentLocation ??= GetCurrentLocation();

        var location = new NavigationLocation(targetUuid, ownerUuid);
        if (TryNavigate(location) == false)
            return false;

        RegisterLocation(location);
        return true;
    }

    /// <summary>
    /// Выполняет переход без изменения стеков истории.
    /// </summary>
    /// <param name="location">Целевая позиция навигации.</param>
    /// <returns>true, если целевая позиция найдена и выбрана; иначе false.</returns>
    private bool TryNavigate(NavigationLocation location)
    {
        var elementUuid = location.OwnerUuid ?? location.TargetUuid;
        var target = _repositoryExplorerVM.FindRepositoryMemberByUuid(elementUuid);
        if (target == null)
            return false;

        var attribute = location.OwnerUuid.HasValue
            ? target.AttributesVMs.FirstOrDefault(x => x.Uuid == location.TargetUuid)
            : null;
        if (location.OwnerUuid.HasValue && attribute == null)
            return false;

        _isNavigationInProgress = true;
        try
        {
            ExpandPath(target);
            SelectTarget(target);
            if (attribute != null)
            {
                _repositoryExplorerVM.FormulaBarVM.SelectedFormulaAttribute = attribute;
                _repositoryExplorerVM.CurrentElementTabIndex = 1;
            }
        }
        finally
        {
            _isNavigationInProgress = false;
        }

        TargetUuid = elementUuid;
        return true;
    }

    /// <summary>
    /// Выполняет переход между стеками истории, пропуская более недоступные позиции.
    /// </summary>
    /// <param name="sourceHistory">Стек, из которого извлекается целевая позиция.</param>
    /// <param name="destinationHistory">Стек, в который помещается текущая позиция.</param>
    private void NavigateThroughHistory(
        Stack<NavigationLocation> sourceHistory,
        Stack<NavigationLocation> destinationHistory)
    {
        _currentLocation ??= GetCurrentLocation();

        while (sourceHistory.TryPop(out var location))
        {
            if (TryNavigate(location) == false)
                continue;

            if (_currentLocation.HasValue)
                destinationHistory.Push(_currentLocation.Value);

            _currentLocation = location;
            RaiseHistoryCanExecuteChanged();
            return;
        }

        RaiseHistoryCanExecuteChanged();
    }

    /// <summary>
    /// Добавляет новую позицию в историю и очищает историю переходов вперёд.
    /// </summary>
    /// <param name="location">Новая текущая позиция.</param>
    private void RegisterLocation(NavigationLocation location)
    {
        if (_currentLocation == location)
            return;

        if (_currentLocation.HasValue)
            _backHistory.Push(_currentLocation.Value);

        _currentLocation = location;
        _forwardHistory.Clear();
        RaiseHistoryCanExecuteChanged();
    }

    /// <summary>
    /// Возвращает позицию, выбранную в обозревателе в настоящий момент.
    /// </summary>
    /// <returns>Текущая позиция либо null, если элемент не выбран.</returns>
    private NavigationLocation? GetCurrentLocation()
    {
        if (_repositoryExplorerVM.SelectedRepositoryMember is not { } owner)
            return null;

        var attribute = _repositoryExplorerVM.FormulaBarVM.SelectedFormulaAttribute;
        return attribute == null
            ? new NavigationLocation(owner.Uuid, null)
            : new NavigationLocation(attribute.Uuid, owner.Uuid);
    }

    /// <summary>
    /// Обновляет доступность команд перехода назад и вперёд.
    /// </summary>
    private void RaiseHistoryCanExecuteChanged()
    {
        _navigateBackCommand?.RaiseCanExecuteChanged();
        _navigateForwardCommand?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Последовательно выделяет представленный в дереве элемент и целевой элемент репозитория.
    /// Для листа сначала выбирается его родительский узел, а затем строка листа в таблице.
    /// </summary>
    /// <param name="target">Целевой элемент навигации.</param>
    private void SelectTarget(IMainEntityVM<IMainEntityModel> target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is TreeLeaveVM leave)
        {
            _repositoryExplorerVM.SelectedRepositoryTreeMember = leave.Parent;
            _repositoryExplorerVM.SelectedRepositoryMember = leave;
            return;
        }

        _repositoryExplorerVM.SelectedRepositoryTreeMember = target;
    }

    /// <summary>
    /// Раскрывает владельцев и родителей целевого элемента в дереве репозитория.
    /// </summary>
    /// <param name="target">Целевой элемент навигации.</param>
    private void ExpandPath(IMainEntityVM<IMainEntityModel> target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target.Model is IContentModel content)
        {
            foreach (var owner in content.AllOwnersRecursive.Values.OfType<IMainEntityModel>())
                Expand(owner.Uuid);
        }

        if (target.Model is IChildrenModel child)
        {
            foreach (var parent in child.AllParentsRecursive.Values.OfType<IMainEntityModel>())
                Expand(parent.Uuid);
        }
    }

    /// <summary>
    /// Раскрывает элемент дерева с указанным UUID, если он представлен в обозревателе.
    /// </summary>
    /// <param name="uuid">UUID раскрываемого элемента.</param>
    private void Expand(Guid uuid)
    {
        if (_repositoryExplorerVM.FindRepositoryMemberByUuid(uuid) is IMainEntityVM entity)
            entity.IsTreeExpanded = true;
    }

    /// <summary>
    /// Позиция в истории навигации по репозиторию.
    /// </summary>
    /// <param name="TargetUuid">UUID элемента либо атрибута.</param>
    /// <param name="OwnerUuid">UUID владельца атрибута либо null для элемента.</param>
    private readonly record struct NavigationLocation(Guid TargetUuid, Guid? OwnerUuid);
}
