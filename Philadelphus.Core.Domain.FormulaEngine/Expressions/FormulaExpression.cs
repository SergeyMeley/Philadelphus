using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Базовый узел AST формулы.
    /// </summary>
    /// <param name="Span">Диапазон узла в исходной формуле.</param>
    public abstract record FormulaExpression(FormulaTextSpan Span);
}
