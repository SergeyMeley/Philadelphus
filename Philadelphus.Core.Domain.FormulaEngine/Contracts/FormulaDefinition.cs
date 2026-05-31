using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Описание формулы: metadata и делегат вычисления.
    /// </summary>
    public sealed class FormulaDefinition
    {
        /// <summary>
        /// Основное имя формулы.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Дополнительные имена и операторные псевдонимы формулы.
        /// </summary>
        public IReadOnlyCollection<string> Aliases { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Человекочитаемое описание формулы.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Категория формулы для группировки в редакторе и справке.
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Ожидаемый тип результата, если он известен статически.
        /// </summary>
        public SystemBaseType? ResultType { get; init; }

        /// <summary>
        /// Пользовательские примеры для подсказок и справки редактора формул.
        /// </summary>
        public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Признак поддержки вызова формулы как объектного метода.
        /// </summary>
        public bool SupportsObjectCall { get; init; }

        /// <summary>
        /// Описание аргументов формулы.
        /// </summary>
        public IReadOnlyList<FormulaArgumentDefinition> Arguments { get; init; } = Array.Empty<FormulaArgumentDefinition>();

        /// <summary>
        /// Делегат, выполняющий вычисление формулы.
        /// </summary>
        public required FormulaEvaluator Evaluator { get; init; }
    }
}
