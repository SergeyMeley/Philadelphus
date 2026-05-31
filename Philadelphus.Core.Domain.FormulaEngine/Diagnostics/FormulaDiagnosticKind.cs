namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Kind of Formula Engine diagnostic event.
    /// </summary>
    public enum FormulaDiagnosticKind
    {
        ParseError,
        EvaluationError,
        PluginLoadError,
        UnexpectedException
    }
}
