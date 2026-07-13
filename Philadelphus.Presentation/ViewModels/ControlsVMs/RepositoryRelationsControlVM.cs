using AutoMapper;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Core.Domain.Relations;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.OtherEntitiesVMs;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs;

/// <summary>
/// Управляет ленивым вычислением и отображением дерева связей текущего элемента.
/// </summary>
public sealed class RepositoryRelationsControlVM : ControlBaseVM
{
    private readonly RepositoryExplorerControlVM _repositoryExplorerVM;
    private readonly IAsyncRelayCommandFactory _asyncCommandFactory;
    private readonly IRelayCommandFactory _commandFactory;
    private readonly RepositoryRelationsService _relationsService = new();
    private IAsyncRelayCommand? _calculateCommand;

    /// <summary>
    /// Инициализирует модель представления дерева связей.
    /// </summary>
    /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
    /// <param name="mapper">Экземпляр AutoMapper.</param>
    /// <param name="logger">Сервис журналирования.</param>
    /// <param name="notificationService">Сервис уведомлений.</param>
    /// <param name="applicationCommandsVM">Модель команд приложения.</param>
    /// <param name="repositoryExplorerVM">Родительская модель обозревателя репозитория.</param>
    /// <param name="navigationVM">Модель навигации по репозиторию.</param>
    /// <param name="commandFactory">Фабрика синхронных команд.</param>
    /// <param name="asyncCommandFactory">Фабрика асинхронных команд.</param>
    public RepositoryRelationsControlVM(
        IServiceProvider serviceProvider,
        IMapper mapper,
        ILogger logger,
        INotificationService notificationService, 
        IApplicationCommandsVM applicationCommandsVM,
        RepositoryExplorerControlVM repositoryExplorerVM, 
        RepositoryNavigationVM navigationVM,
        IRelayCommandFactory commandFactory,
        IAsyncRelayCommandFactory asyncCommandFactory)
        : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
    {
        _repositoryExplorerVM = repositoryExplorerVM ?? throw new ArgumentNullException(nameof(repositoryExplorerVM));
        NavigationVM = navigationVM ?? throw new ArgumentNullException(nameof(navigationVM));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _asyncCommandFactory = asyncCommandFactory ?? throw new ArgumentNullException(nameof(asyncCommandFactory));
    }

    /// <summary>
    /// Сервис навигации по объектам дерева связей.
    /// </summary>
    public RepositoryNavigationVM NavigationVM { get; }

    /// <summary>
    /// Корневые узлы рассчитанного дерева связей.
    /// </summary>
    public ObservableCollection<RepositoryRelationVM> Roots { get; } = new();

    /// <summary>
    /// Команда пересчета непосредственных связей текущего элемента.
    /// </summary>
    public IAsyncRelayCommand CalculateCommand => _calculateCommand ??= _asyncCommandFactory.Create(CalculateAsync,
        _ => _repositoryExplorerVM.SelectedRepositoryMember != null);

    /// <summary>
    /// Очищает рассчитанное дерево связей.
    /// </summary>
    public void Reset() => Roots.Clear();

    /// <summary>
    /// Пересчитывает корневой уровень дерева для выбранного элемента.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>Задача, представляющая асинхронный пересчет.</returns>
    private async Task CalculateAsync(object parameter)
    {
        var selected = _repositoryExplorerVM.SelectedRepositoryMember?.Model;
        if (selected == null) return;
        var selectedUuid = selected.Uuid;
        var relations = await Task.Run(() => _relationsService.GetDirectRelations(
            _repositoryExplorerVM.PhiladelphusRepositoryVM.Model, selected));
        if (_repositoryExplorerVM.SelectedRepositoryMember?.Uuid != selectedUuid) return;
        Roots.Clear();
        foreach (var relation in relations) Roots.Add(CreateNode(relation));
    }

    /// <summary>
    /// Создает узел с командами ленивой загрузки и навигации.
    /// </summary>
    /// <param name="relation">Связь, для которой создается узел.</param>
    /// <returns>Новый узел дерева связей.</returns>
    private RepositoryRelationVM CreateNode(RepositoryRelationModel relation)
    {
        return new RepositoryRelationVM(relation, async node =>
        {
            var relations = await Task.Run(() => _relationsService.GetDirectRelations(
                _repositoryExplorerVM.PhiladelphusRepositoryVM.Model, node.Relation.Target));
            foreach (var child in relations) node.Children.Add(CreateNode(child));
        }, _asyncCommandFactory, _commandFactory.Create(_ =>
            NavigationVM.Navigate(relation.Target.Uuid, relation.NavigationOwnerUuid)));
    }
}
