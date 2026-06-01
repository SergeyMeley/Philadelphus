namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Получает диагностику Formula Engine для уведомлений UI и журнала приложения.
    /// </summary>
    public interface IFormulaDiagnosticsReporter
    {
        /// <summary>
        /// Обрабатывает диагностическое событие Formula Engine.
        /// </summary>
        /// <param name="diagnostic">Диагностическое событие.</param>
        void Report(FormulaDiagnostic diagnostic);
    }
}
