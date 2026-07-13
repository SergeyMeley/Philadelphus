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
    private bool _isLoaded;
    private bool _isLoading;

    /// <summary>
    /// Инициализирует модель представления связи объекта репозитория.
    /// </summary>
    /// <param name="relation">Доменная модель связи.</param>
    /// <param name="loader">Функция загрузки дочерних связей.</param>
    /// <param name="commandFactory">Фабрика асинхронных команд.</param>
    /// <param name="navigateCommand">Команда перехода к связанному объекту.</param>
    public RepositoryRelationVM(RepositoryRelationModel relation, Func<RepositoryRelationVM, Task> loader,
        IAsyncRelayCommandFactory commandFactory, IRelayCommand navigateCommand)
    {
        Relation = relation;
        _loader = loader;
        LoadChildrenCommand = commandFactory.Create(_ => LoadAsync(), _ => !_isLoaded && !_isLoading);
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
    /// Команда загрузки непосредственных дочерних связей.
    /// </summary>
    public IAsyncRelayCommand LoadChildrenCommand { get; }

    /// <summary>
    /// Команда перехода к связанному объекту.
    /// </summary>
    public IRelayCommand NavigateCommand { get; }

    /// <summary>
    /// Загружает непосредственные дочерние связи узла один раз.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную загрузку.</returns>
    private async Task LoadAsync()
    {
        if (_isLoaded) return;
        _isLoading = true;
        OnPropertyChanged(nameof(IsLoading));
        try { await _loader(this); _isLoaded = true; }
        finally { _isLoading = false; OnPropertyChanged(nameof(IsLoading)); LoadChildrenCommand.RaiseCanExecuteChanged(); }
    }

    /// <summary>
    /// Признак выполняющейся загрузки дочерних связей.
    /// </summary>
    public bool IsLoading => _isLoading;
}
