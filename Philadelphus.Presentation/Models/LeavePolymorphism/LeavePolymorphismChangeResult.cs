using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Presentation.Models.LeavePolymorphism;

/// <summary>
/// Описывает результат интерактивной обработки изменения полиморфного листа.
/// </summary>
public sealed class LeavePolymorphismChangeResult
{
    /// <summary>
    /// Создаёт неизменяемый снимок результа операции.
    /// </summary>
    /// <param name="cascadeProcessed">Признак применения одной из ветвей каскада.</param>
    /// <param name="createdLeaves">Созданные заменяющие листы и их предки.</param>
    public LeavePolymorphismChangeResult(
        bool cascadeProcessed,
        IEnumerable<TreeLeaveModel> createdLeaves)
    {
        ArgumentNullException.ThrowIfNull(createdLeaves);

        CascadeProcessed = cascadeProcessed;
        CreatedLeaves = createdLeaves.ToList().AsReadOnly();
    }

    /// <summary>
    /// Указывает, что изменение затронуло разрешённых наследников.
    /// </summary>
    public bool CascadeProcessed { get; }

    /// <summary>
    /// Листы, созданные для сохранения прежних значений наследников.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> CreatedLeaves { get; }
}
