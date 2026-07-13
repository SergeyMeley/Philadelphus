using Philadelphus.Presentation.Models.Dialogs;

namespace Philadelphus.Presentation.Services.Interfaces;

/// <summary>
/// Задаёт контракт диалога подтверждения удаления элемента, на который существуют ссылки.
/// </summary>
public interface IRelationDeletionConfirmationService
{
    /// <summary>
    /// Показывает предупреждение со списком связей, которые будут сброшены при удалении.
    /// </summary>
    /// <param name="elementDisplayName">Текстовое представление удаляемого элемента.</param>
    /// <param name="relations">Связи, которые будут сброшены.</param>
    /// <returns>true, если пользователь подтвердил удаление; иначе false.</returns>
    Task<bool> ConfirmAsync(
        string elementDisplayName,
        IReadOnlyList<RelationDeletionWarningRow> relations);
}
