using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Условное выражение формулы condition ? valueIfTrue : valueIfFalse.
    /// </summary>
    public sealed record ConditionalFormulaExpression(
        FormulaExpression Condition,
        FormulaExpression WhenTrue,
        FormulaExpression WhenFalse,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
