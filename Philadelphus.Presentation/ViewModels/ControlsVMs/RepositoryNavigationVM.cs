using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Управляет переходами к элементам и атрибутам открытого репозитория.
/// </summary>
public sealed class RepositoryNavigationVM : ControlBaseVM
{
    private readonly RepositoryExplorerControlVM _repositoryExplorerVM;
    private Guid? _targetUuid;

    /// <summary>
    /// Инициализирует модель представления навигации по репозиторию.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
    /// <param name="mapper">Экземпляр AutoMapper.</param>
    /// <param name="logger">Сервис журналирования.</param>
    /// <param name="notificationService">Сервис уведомлений.</param>
    /// <param name="applicationCommandsVM">Модель команд приложения.</param>
    /// <param name="repositoryExplorerVM">Родительская модель обозревателя репозитория.</param>
    public RepositoryNavigationVM(
        IServiceProvider serviceProvider, 
        IMapper mapper,
        ILogger logger,
        INotificationService notificationService, 
        IApplicationCommandsVM applicationCommandsVM,
        RepositoryExplorerControlVM repositoryExplorerVM)
        : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
    {
        ArgumentNullException.ThrowIfNull(repositoryExplorerVM);

        _repositoryExplorerVM = repositoryExplorerVM;
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
        var elementUuid = ownerUuid ?? targetUuid;
        var target = _repositoryExplorerVM.FindRepositoryMemberByUuid(elementUuid);
        if (target == null) return false;

        _repositoryExplorerVM.SelectedRepositoryMember = target;
        if (ownerUuid.HasValue)
        {
            _repositoryExplorerVM.FormulaBarVM.SelectedFormulaAttribute =
                target.AttributesVMs.FirstOrDefault(x => x.Uuid == targetUuid);
            _repositoryExplorerVM.CurrentElementTabIndex = 1;
        }

        TargetUuid = elementUuid;
        return true;
    }
}
