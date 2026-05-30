using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Объектный вызов функции от результата другого выражения.
    /// </summary>
    public sealed record ObjectMethodCallFormulaExpression(
        FormulaExpression Target,
        string MethodName,
        IReadOnlyList<FormulaExpression> Arguments,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
