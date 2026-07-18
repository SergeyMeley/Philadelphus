using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

/// <summary>
/// Хранит презентационное состояние строки выбора полиморфного родителя.
/// </summary>
public sealed class LeavePolymorphismAttributeVM : ViewModelBase
{
    private readonly LeavePolymorphismAttributeModel _model;
    private TreeLeaveModel? _selectedCandidate;

    /// <summary>
    /// Инициализирует состояние runtime-атрибута.
    /// </summary>
    /// <param name="model">Доменная модель служебного атрибута.</param>
    public LeavePolymorphismAttributeVM(LeavePolymorphismAttributeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
    }

    /// <summary>
    /// Лист, атрибуты которого можно заполнить по выбранному родителю.
    /// </summary>
    public TreeLeaveModel? RecipientLeave => _model.Owner as TreeLeaveModel;

    /// <summary>
    /// Текущий статус вычисления связи.
    /// </summary>
    public LeavePolymorphismStatus Status => _model.Status;

    /// <summary>
    /// Краткое текстовое представление результата поиска.
    /// </summary>
    public string DisplayText => Status switch
    {
        LeavePolymorphismStatus.Resolved => _model.Value?.Name ?? "Не найден",
        LeavePolymorphismStatus.NotFound => "Не найден",
        LeavePolymorphismStatus.Ambiguous => "Неоднозначно",
        LeavePolymorphismStatus.Invalid => "Ошибка",
        _ => Status.ToString()
    };

    /// <summary>
    /// Найденные кандидаты для ручного заполнения атрибутов.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> Candidates => _model.Candidates;

    /// <summary>
    /// Временно выбранный кандидат; выбор не изменяет доменную модель.
    /// </summary>
    public TreeLeaveModel? SelectedCandidate
    {
        get => _selectedCandidate;
        set
        {
            var candidate = value == null
                ? null
                : Candidates.FirstOrDefault(x => x.Uuid == value.Uuid);

            if (SetProperty(ref _selectedCandidate, candidate))
                OnPropertyChanged(nameof(CanApplyCandidate));
        }
    }

    /// <summary>
    /// Указывает, что выбранного кандидата можно применить к листу.
    /// </summary>
    public bool CanApplyCandidate => RecipientLeave != null && SelectedCandidate != null;

    /// <summary>
    /// Указывает, что для листа можно создать отсутствующую цепочку родителей.
    /// </summary>
    public bool CanCreateParent =>
        RecipientLeave != null && Status == LeavePolymorphismStatus.NotFound;

    /// <summary>
    /// Синхронизирует UI с повторно вычисленным результатом и сбрасывает временный выбор.
    /// </summary>
    public void NotifyResolutionChanged()
    {
        SelectedCandidate = null;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(DisplayText));
        OnPropertyChanged(nameof(Candidates));
        OnPropertyChanged(nameof(CanCreateParent));
    }
}
