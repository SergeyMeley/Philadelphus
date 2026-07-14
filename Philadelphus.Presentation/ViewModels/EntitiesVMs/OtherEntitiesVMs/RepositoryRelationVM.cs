using System.Collections.ObjectModel;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Core.Domain.Relations;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.OtherEntitiesVMs;

/// <summary>
/// Представляет вычисленную связь объекта репозитория.
/// </summary>
public sealed class RepositoryRelationVM : ViewModelBase
{
    private readonly Func<RepositoryRelationVM, Task> _loader;
    private Task? _loadTask;
    private bool _isLoaded;
    private bool _isLoading;
    private bool _isExpanded;

    /// <summary>
    /// Инициализирует модель представления связи объекта репозитория.
    /// </summary>
    /// <param name="relation">Доменная модель связи.</param>
    /// <param name="loader">Функция загрузки дочерних связей.</param>
    /// <param name="navigateCommand">Команда перехода к связанному объекту.</param>
    public RepositoryRelationVM(RepositoryRelationModel relation, Func<RepositoryRelationVM, Task> loader,
        IRelayCommand navigateCommand)
    {
        Relation = relation;
        _loader = loader;
        NavigateCommand = navigateCommand;
    }

    /// <summary>
    /// Связь, представленная узлом.
    /// </summary>
    public RepositoryRelationModel Relation { get; }

    /// <summary>
    /// Отображаемое наименование связанного объекта.
    /// </summary>
    public string Element => Relation.DisplayName;

    /// <summary>
    /// Отображаемое наименование типа связи.
    /// </summary>
    public string RelationType => Relation.TypeDisplayName;

    /// <summary>
    /// Признак блокировки удаления исходного объекта.
    /// </summary>
    public bool BlocksDeletion => Relation.BlocksSourceDeletion;

    /// <summary>
    /// Наименование группы блокировки удаления.
    /// </summary>
    public string Group => BlocksDeletion ? "Блокирующие удаление" : "Не блокирующие удаление";

    /// <summary>
    /// Лениво загруженные дочерние связи.
    /// </summary>
    public ObservableCollection<RepositoryRelationVM> Children { get; } = new();

    /// <summary>
    /// Команда перехода к связанному объекту.
    /// </summary>
    public IRelayCommand NavigateCommand { get; }

    /// <summary>
    /// Признак раскрытия узла в дереве связей.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value) && value)
                _ = PrepareNextLevelAsync();
        }
    }

    /// <summary>
    /// Загружает непосредственные дочерние связи узла один раз.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную загрузку.</returns>
    public Task EnsureChildrenLoadedAsync()
    {
        if (_isLoaded)
            return Task.CompletedTask;

        return _loadTask ??= LoadAsync();
    }

    /// <summary>
    /// При раскрытии узла подготавливает дочерние связи каждого отображаемого потомка.
    /// </summary>
    /// <returns>Задача предварительной загрузки следующего уровня.</returns>
    private async Task PrepareNextLevelAsync()
    {
        await EnsureChildrenLoadedAsync();
        await Task.WhenAll(Children.Select(x => x.EnsureChildrenLoadedAsync()));
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        OnPropertyChanged(nameof(IsLoading));
        try
        {
            await _loader(this);
            _isLoaded = true;
        }
        finally
        {
            _loadTask = null;
            _isLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    /// <summary>
    /// Признак выполняющейся загрузки дочерних связей.
    /// </summary>
    public bool IsLoading => _isLoading;
}
