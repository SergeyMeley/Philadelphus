namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Запрос открытия редактора формул из интерфейса репозитория.
    /// </summary>
    /// <param name="RepositoryExplorerControlVM">Обозреватель репозитория, задающий контекст формулы.</param>
    /// <param name="FormulaText">Текущий текст формулы.</param>
    public sealed record FormulaEditorOpenRequest(
        RepositoryExplorerControlVM RepositoryExplorerControlVM,
        string? FormulaText);
}
