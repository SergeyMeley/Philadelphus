namespace Philadelphus.Presentation.Models.Dialogs;

/// <summary>
/// Представляет строку перечня связей, которые будут сброшены при удалении элемента.
/// </summary>
/// <param name="Element">Текстовое представление использующего элемента.</param>
/// <param name="RelationType">Тип связи с удаляемым элементом.</param>
public sealed record RelationDeletionWarningRow(string Element, string RelationType);
