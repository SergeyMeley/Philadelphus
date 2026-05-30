using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Базовый узел AST формулы.
    /// </summary>
    public abstract record FormulaExpression(FormulaTextSpan Span);
}
