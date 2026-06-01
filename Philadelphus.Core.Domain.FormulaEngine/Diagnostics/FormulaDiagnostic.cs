using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Diagnostics
{
    /// <summary>
    /// Диагностическое событие Formula Engine без привязки ядра к конкретному логгеру или UI.
    /// </summary>
    public sealed class FormulaDiagnostic
    {
        /// <summary>
        /// Тип диагностического события.
        /// </summary>
        public required FormulaDiagnosticKind Kind { get; init; }

        /// <summary>
        /// Ошибка, из-за которой создано диагностическое событие.
        /// </summary>
        public required FormulaError Error { get; init; }
    }
}
