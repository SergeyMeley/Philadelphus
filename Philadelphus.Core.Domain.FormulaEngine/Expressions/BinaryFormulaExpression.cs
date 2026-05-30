using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Бинарное выражение формулы.
    /// </summary>
    public sealed record BinaryFormulaExpression(
        FormulaExpression Left,
        string Operator,
        FormulaExpression Right,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
