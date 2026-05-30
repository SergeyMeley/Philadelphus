using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Вызов функции формулы.
    /// </summary>
    public sealed record FunctionCallFormulaExpression(
        string Name,
        IReadOnlyList<FormulaExpression> Arguments,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
