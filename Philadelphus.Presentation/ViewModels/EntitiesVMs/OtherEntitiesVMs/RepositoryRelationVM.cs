using System.Collections.ObjectModel;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Core.Domain.Relations;
using Philadelphus.Presentation.Services.StateVisibility;

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
    /// <param name="sourceName">Наименование исходного элемента связи.</param>
    /// <param name="loader">Функция загрузки дочерних связей.</param>
    public RepositoryRelationVM(
        RepositoryRelationModel relation,
        string sourceName,
        Func<RepositoryRelationVM, Task> loader)
    {
        Relation = relation;
        SourceName = sourceName;
        _loader = loader;
    }

    /// <summary>
    /// Связь, представленная узлом.
    /// </summary>
    public RepositoryRelationModel Relation { get; }

    /// <summary>
    /// Наименование исходного элемента связи.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// Отображаемое наименование связанного объекта.
    /// </summary>
    public string Element => Relation.DisplayName;

    /// <summary>
    /// Наименование связанного объекта.
    /// </summary>
    public string ElementName => Relation.Target.Name;

    /// <summary>
    /// Отображаемое наименование типа связи.
    /// </summary>
    public string RelationType =>
        Relation.Type == RepositoryRelationType.Content
        && Relation.Target is ElementAttributeModel
            ? $"{Relation.TypeDisplayName} (атрибут)"
            : Relation.TypeDisplayName;

    /// <summary>
    /// Признак входящей ссылки на текущий элемент из атрибута.
    /// </summary>
    public bool IsAttributeReference =>
        Relation.Target is ElementAttributeModel && Relation.BlocksSourceDeletion;

    /// <summary>
    /// Наименование типа атрибутной связи в форме, согласованной со словом «является».
    /// </summary>
    public string AttributeRelationType => Relation.Type switch
    {
        RepositoryRelationType.AttributeDataType => "типом данных",
        RepositoryRelationType.AttributeValue => "значением",
        RepositoryRelationType.AttributeCollectionValue => "одним из значений",
        RepositoryRelationType.FormulaReference => "ссылкой из формулы",
        _ => RelationType
    };

    /// <summary>
    /// Наименование атрибута, содержащего входящую ссылку.
    /// </summary>
    public string AttributeName =>
        Relation.Target is ElementAttributeModel attribute ? attribute.Name : string.Empty;

    /// <summary>
    /// Элемент, отображаемый как цель связи. Для входящей атрибутной ссылки — владелец атрибута.
    /// </summary>
    public IMainEntityModel DisplayedElement =>
        IsAttributeReference
        && Relation.Target is ElementAttributeModel attribute
        && attribute.Owner is IMainEntityModel owner
            ? owner
            : Relation.Target;

    /// <summary>
    /// Признак блокировки удаления исходного объекта.
    /// </summary>
    public bool BlocksDeletion => Relation.BlocksSourceDeletion;

    /// <summary>
    /// Агрегированное состояние родителей и владельцев связанного элемента.
    /// </summary>
    public State ParentOwnerAggregateState =>
        StateVisibilityInfoBuilder.Build(DisplayedElement).ParentOwnerState ?? State.SavedOrLoaded;

    /// <summary>
    /// Состояние связанного элемента.
    /// </summary>
    public State State => DisplayedElement.State;

    /// <summary>
    /// Агрегированное состояние дочерних элементов и содержимого связанного элемента.
    /// </summary>
    public State ChildContentAggregateState =>
        StateVisibilityInfoBuilder.Build(DisplayedElement).ChildContentState ?? State.SavedOrLoaded;

    /// <summary>
    /// Подсказка с расшифровкой состояний связанного элемента.
    /// </summary>
    public string StateVisibilityToolTip => StateVisibilityInfoBuilder.Build(DisplayedElement).ToolTip;

    /// <summary>
    /// Лениво загруженные дочерние связи.
    /// </summary>
    public ObservableCollection<RepositoryRelationVM> Children { get; } = new();

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
