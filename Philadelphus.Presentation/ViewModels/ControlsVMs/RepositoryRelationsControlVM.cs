using AutoMapper;
using Philadelphus.Core.Domain.Entities.MainEntities;
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
    private readonly IRelayCommandFactory _commandFactory;
    private readonly IRepositoryRelationsService _relationsService;
    private int _calculationVersion;
    private bool _isLoading;
    private RepositoryRelationVM? _selectedRelation;
    private IRelayCommand? _navigateToSelectedRelationCommand;

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
    /// <param name="relationsService">Сервис вычисления связей репозитория.</param>
    public RepositoryRelationsControlVM(
        IServiceProvider serviceProvider,
        IMapper mapper,
        ILogger logger,
        INotificationService notificationService, 
        IApplicationCommandsVM applicationCommandsVM,
        RepositoryExplorerControlVM repositoryExplorerVM, 
        RepositoryNavigationVM navigationVM,
        IRelayCommandFactory commandFactory,
        IRepositoryRelationsService relationsService)
        : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
    {
        ArgumentNullException.ThrowIfNull(repositoryExplorerVM);
        ArgumentNullException.ThrowIfNull(navigationVM);
        ArgumentNullException.ThrowIfNull(commandFactory);
        ArgumentNullException.ThrowIfNull(relationsService);

        _repositoryExplorerVM = repositoryExplorerVM;
        NavigationVM = navigationVM;
        _commandFactory = commandFactory;
        _relationsService = relationsService;
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
    /// Выбранный узел дерева связей.
    /// </summary>
    public RepositoryRelationVM? SelectedRelation
    {
        get => _selectedRelation;
        set
        {
            if (SetProperty(ref _selectedRelation, value))
                _navigateToSelectedRelationCommand?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Команда перехода к элементу выбранной связи.
    /// </summary>
    public IRelayCommand NavigateToSelectedRelationCommand =>
        _navigateToSelectedRelationCommand ??= _commandFactory.Create(
            obj =>
            {
                if (SelectedRelation == null)
                    return;

                var relation = SelectedRelation.Relation;
                NavigationVM.Navigate(relation.Target.Uuid, relation.NavigationOwnerUuid);
            },
            ce =>
            {
                return SelectedRelation != null;
            });

    /// <summary>
    /// Признак выполняющегося расчёта дерева связей.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Очищает рассчитанное дерево связей.
    /// </summary>
    public void Reset()
    {
        Interlocked.Increment(ref _calculationVersion);
        SelectedRelation = null;
        Roots.Clear();
        IsLoading = false;
    }

    /// <summary>
    /// Автоматически запускает расчёт дерева связей для выбранного элемента.
    /// </summary>
    public void Refresh()
    {
        _ = StartRefresh();
    }

    /// <summary>
    /// Запускает новую версию расчёта для текущего выбранного элемента.
    /// </summary>
    /// <returns>Задача, представляющая асинхронный расчёт.</returns>
    private Task StartRefresh()
    {
        var selected = _repositoryExplorerVM.SelectedRepositoryMember?.Model;
        var version = Interlocked.Increment(ref _calculationVersion);
        SelectedRelation = null;
        Roots.Clear();

        if (selected == null)
        {
            IsLoading = false;
            return Task.CompletedTask;
        }

        IsLoading = true;
        return RefreshAsync(version, selected);
    }

    /// <summary>
    /// Рассчитывает корневые связи и заранее загружает их непосредственных потомков.
    /// </summary>
    /// <param name="version">Версия расчёта для защиты от устаревшего результата.</param>
    /// <param name="selected">Элемент, для которого рассчитываются связи.</param>
    /// <returns>Задача, представляющая асинхронный расчёт.</returns>
    private async Task RefreshAsync(int version, IMainEntityModel selected)
    {
        try
        {
            await Task.Yield();
            if (version != Volatile.Read(ref _calculationVersion))
                return;

            var relations = await Task.Run(() => _relationsService.GetDirectRelations(
                _repositoryExplorerVM.PhiladelphusRepositoryVM.Model, selected));
            var nodes = relations.Select(x => CreateNode(x, selected.Name)).ToList();
            await Task.WhenAll(nodes.Select(x => x.EnsureChildrenLoadedAsync()));

            if (version != Volatile.Read(ref _calculationVersion)
                || _repositoryExplorerVM.SelectedRepositoryMember?.Uuid != selected.Uuid)
                return;

            foreach (var node in nodes)
                Roots.Add(node);
        }
        catch (Exception ex)
        {
            if (version == Volatile.Read(ref _calculationVersion))
                _logger.Error(ex, "Ошибка расчёта дерева связей элемента {ElementUuid}.", selected.Uuid);
        }
        finally
        {
            if (version == Volatile.Read(ref _calculationVersion))
                IsLoading = false;
        }
    }

    /// <summary>
    /// Создает узел с ленивой загрузкой дочерних связей.
    /// </summary>
    /// <param name="relation">Связь, для которой создается узел.</param>
    /// <param name="sourceName">Наименование исходного элемента связи.</param>
    /// <returns>Новый узел дерева связей.</returns>
    private RepositoryRelationVM CreateNode(RepositoryRelationModel relation, string sourceName)
    {
        return new RepositoryRelationVM(relation, sourceName, async node =>
        {
            try
            {
                var relations = await Task.Run(() => _relationsService.GetDirectRelations(
                    _repositoryExplorerVM.PhiladelphusRepositoryVM.Model, node.Relation.Target));
                foreach (var child in relations)
                    node.Children.Add(CreateNode(child, node.Relation.Target.Name));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка расчёта дочерних связей элемента {ElementUuid}.", node.Relation.Target.Uuid);
            }
        });
    }

}
