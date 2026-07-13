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
    private Guid? _targetUuid;
    private IRelayCommand? _navigateToAttributeDataTypeCommand;
    private IRelayCommand? _navigateToAttributeValueCommand;

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
        var elementUuid = ownerUuid ?? targetUuid;
        var target = _repositoryExplorerVM.FindRepositoryMemberByUuid(elementUuid);
        if (target == null) return false;

        ExpandPath(target);
        SelectTarget(target);
        if (ownerUuid.HasValue)
        {
            _repositoryExplorerVM.FormulaBarVM.SelectedFormulaAttribute =
                target.AttributesVMs.FirstOrDefault(x => x.Uuid == targetUuid);
            _repositoryExplorerVM.CurrentElementTabIndex = 1;
        }

        TargetUuid = elementUuid;
        return true;
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
}
