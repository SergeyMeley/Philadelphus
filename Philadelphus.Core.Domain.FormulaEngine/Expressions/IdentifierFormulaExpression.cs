using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Идентификатор формулы.
    /// </summary>
    public sealed record IdentifierFormulaExpression(
        string Name,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
