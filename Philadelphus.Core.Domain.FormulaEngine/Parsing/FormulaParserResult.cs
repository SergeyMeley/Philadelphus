using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Expressions;

namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Результат синтаксического анализа формулы.
    /// </summary>
    public sealed class FormulaParserResult
    {
        public FormulaParserResult(
            FormulaExpression? expression,
            IReadOnlyList<FormulaError> errors)
        {
            Expression = expression;
            Errors = errors;
        }

        /// <summary>
        /// Признак успешного синтаксического анализа.
        /// </summary>
        public bool IsSuccess => Expression is not null && Errors.Count == 0;

        /// <summary>
        /// Корневое выражение AST.
        /// </summary>
        public FormulaExpression? Expression { get; }

        /// <summary>
        /// Ошибки синтаксического анализа.
        /// </summary>
        public IReadOnlyList<FormulaError> Errors { get; }
    }
}
