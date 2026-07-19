using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Presentation.Models.LeavePolymorphism;

/// <summary>
/// Описывает результат подтверждаемого заполнения по выбранному родительскому листу.
/// </summary>
public sealed class LeavePolymorphismManualFillResult
{
    /// <summary>
    /// Создаёт неизменяемый результат ручного заполнения.
    /// </summary>
    /// <param name="applied">Признак подтверждения и применения операции.</param>
    /// <param name="changedAttributes">Фактически изменённые атрибуты получателя.</param>
    public LeavePolymorphismManualFillResult(
        bool applied,
        IEnumerable<ElementAttributeModel> changedAttributes)
    {
        ArgumentNullException.ThrowIfNull(changedAttributes);

        Applied = applied;
        ChangedAttributes = changedAttributes.ToList().AsReadOnly();
    }

    /// <summary>
    /// Указывает, что пользователь подтвердил операцию и заполнение было выполнено.
    /// </summary>
    public bool Applied { get; }

    /// <summary>
    /// Атрибуты, значения которых были изменены применённым заполнением.
    /// </summary>
    public IReadOnlyList<ElementAttributeModel> ChangedAttributes { get; }
}
