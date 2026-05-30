using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Результат лексического анализа формулы.
    /// </summary>
    public sealed class FormulaTokenizerResult
    {
        public FormulaTokenizerResult(
            IReadOnlyList<FormulaToken> tokens,
            IReadOnlyList<FormulaError> errors)
        {
            Tokens = tokens;
            Errors = errors;
        }

        /// <summary>
        /// Признак успешного лексического анализа.
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Найденные токены формулы.
        /// </summary>
        public IReadOnlyList<FormulaToken> Tokens { get; }

        /// <summary>
        /// Ошибки лексического анализа.
        /// </summary>
        public IReadOnlyList<FormulaError> Errors { get; }
    }
}
