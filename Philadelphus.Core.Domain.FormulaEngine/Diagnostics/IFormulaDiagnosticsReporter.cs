namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Receives Formula Engine diagnostics for UI notifications and application logging.
    /// </summary>
    public interface IFormulaDiagnosticsReporter
    {
        void Report(FormulaDiagnostic diagnostic);
    }
}
