using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик формул создания коллекций значений атрибутов.
    /// </summary>
    public sealed class CollectionFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определение функции МАССИВ.
        /// </summary>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "МАССИВ",
                Description = "Создаёт коллекцию из результатов выражений, возвращающих листья дерева.",
                Category = "Коллекции",
                ResultType = SystemBaseType.USER_DEFINED,
                Examples = ["=МАССИВ([uuid1];[uuid2])"],
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "лист",
                        Description = "Выражение, возвращающее лист дерева.",
                        ExpectedType = SystemBaseType.USER_DEFINED,
                        IsRequired = false
                    }
                ],
                Evaluator = (_, arguments) => Evaluate(arguments)
            };
        }

        private static FormulaResult Evaluate(IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Any(x => x.TreeLeave is null))
            {
                return FormulaResult.Failure(new FormulaError
                {
                    Code = FormulaErrorCode.TypeMismatch,
                    Message = "Каждый аргумент формулы 'МАССИВ' должен возвращать лист дерева.",
                    FunctionOrOperator = "МАССИВ"
                });
            }

            return FormulaResult.FromTreeLeaves(arguments.Select(x => x.TreeLeave!));
        }
    }
}
