using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

/// <summary>
/// Хранит презентационное состояние строки выбора полиморфного родителя.
/// </summary>
public sealed class LeavePolymorphismAttributeVM : ViewModelBase
{
    private readonly LeavePolymorphismAttributeModel _model;
    private TreeLeaveModel? _selectedCandidate;
    private IAsyncRelayCommand? _parentSelectionCommand;

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
    /// Узел или лист, атрибуты которого можно заполнить по выбранному родителю.
    /// </summary>
    public IAttributeOwnerModel? Recipient => _model.Owner as IAttributeOwnerModel;

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
    /// Активные листья прямого родительского узла, доступные для ручного заполнения.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> AvailableParents =>
        _model.ValuesList?
            .Where(x => x.AuditInfo.IsDeleted == false
                && x.State is not State.ForSoftDelete
                    and not State.ForHardDelete
                    and not State.SoftDeleted)
            .ToList()
        ?? [];

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
                : AvailableParents.FirstOrDefault(x => x.Uuid == value.Uuid);

            if (SetProperty(ref _selectedCandidate, candidate))
            {
                OnPropertyChanged(nameof(CanApplyCandidate));
                if (candidate != null)
                    _parentSelectionCommand?.Execute(this);
            }
        }
    }

    /// <summary>
    /// Указывает, что выбранного кандидата можно применить к листу.
    /// </summary>
    public bool CanApplyCandidate => Recipient != null && SelectedCandidate != null;

    /// <summary>
    /// Объясняет, почему для текущего состояния нельзя создать полиморфного родителя.
    /// Пустое значение означает, что операция доступна.
    /// </summary>
    public string? ParentCreationBlockReason =>
        Recipient == null
            ? "Не удалось определить узел или лист, для которого требуется создать родителя."
            : Status switch
            {
                LeavePolymorphismStatus.NotFound => null,
                LeavePolymorphismStatus.Resolved =>
                    "Полиморфный родитель уже найден. Создание дополнительного листа не требуется.",
                LeavePolymorphismStatus.Ambiguous =>
                    "Полиморфный родитель определён неоднозначно. Сначала устраните дубликаты или измените значения атрибутов.",
                LeavePolymorphismStatus.Invalid =>
                    "Невозможно создать полиморфного родителя: значения унаследованных атрибутов содержат ошибки или неразрешённые ссылки.",
                _ => "Невозможно создать полиморфного родителя в текущем состоянии."
            };

    /// <summary>
    /// Выбирает родителя по ссылке, введённой стандартным редактором значения атрибута.
    /// </summary>
    /// <param name="text">Ссылка вида <c>[uuid]</c> или <c>=[uuid]</c>.</param>
    /// <returns><see langword="true"/>, если ссылка указывает на допустимый родительский лист.</returns>
    public bool TrySelectCandidate(string? text)
    {
        var reference = text?.Trim() ?? string.Empty;
        if (reference.StartsWith("=", StringComparison.Ordinal))
            reference = reference[1..].TrimStart();

        if (FormulaReferenceParser.TryParseTreeLeaveReference(reference, out var uuid) == false)
            return false;

        var candidate = AvailableParents.FirstOrDefault(x => x.Uuid == uuid);
        if (candidate == null)
            return false;

        SelectedCandidate = candidate;
        return true;
    }

    /// <summary>
    /// Назначает платформо-независимую команду, автоматически выполняемую после выбора родителя.
    /// </summary>
    /// <param name="command">Команда подтверждаемого заполнения атрибутов.</param>
    internal void SetParentSelectionCommand(IAsyncRelayCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _parentSelectionCommand = command;
    }

    /// <summary>
    /// Синхронизирует UI с повторно вычисленным результатом и сбрасывает временный выбор.
    /// </summary>
    public void NotifyResolutionChanged()
    {
        SelectedCandidate = null;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(DisplayText));
        OnPropertyChanged(nameof(AvailableParents));
        OnPropertyChanged(nameof(ParentCreationBlockReason));
    }
}
