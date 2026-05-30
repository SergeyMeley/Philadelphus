using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Условное выражение формулы condition ? valueIfTrue : valueIfFalse.
    /// </summary>
    /// <param name="Condition">Выражение условия.</param>
    /// <param name="WhenTrue">Выражение, вычисляемое при истинном условии.</param>
    /// <param name="WhenFalse">Выражение, вычисляемое при ложном условии.</param>
    /// <param name="Span">Диапазон выражения в исходной формуле.</param>
    public sealed record ConditionalFormulaExpression(
        FormulaExpression Condition,
        FormulaExpression WhenTrue,
        FormulaExpression WhenFalse,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
