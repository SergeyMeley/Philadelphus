using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Diagnostic event emitted by Formula Engine without binding the core to a concrete logger or UI.
    /// </summary>
    public sealed class FormulaDiagnostic
    {
        public required FormulaDiagnosticKind Kind { get; init; }

        public required FormulaError Error { get; init; }
    }
}
