using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик условной формулы ЕСЛИ и общей семантики условного выбора.
    /// </summary>
    public sealed class ConditionalFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения условных формул этапа 1.
        /// </summary>
        /// <returns>Коллекция условных формул.</returns>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "ЕСЛИ",
                Aliases = ["?:"],
                Description = "Возвращает одно из двух значений в зависимости от логического условия.",
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "условие",
                        Description = "Логическое условие.",
                        ExpectedType = SystemBaseType.BOOL
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "еслиИстина",
                        Description = "Значение, возвращаемое при истинном условии."
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "еслиЛожь",
                        Description = "Значение, возвращаемое при ложном условии."
                    }
                ],
                Evaluator = (_, arguments) => EvaluateIf(arguments)
            };
        }

        /// <summary>
        /// Вычисляет именованную формулу ЕСЛИ по уже вычисленным аргументам.
        /// </summary>
        /// <param name="arguments">Аргументы формулы ЕСЛИ.</param>
        /// <returns>Выбранный результат или ошибка условия.</returns>
        private static FormulaResult EvaluateIf(IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count != 3)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'ЕСЛИ' ожидает три аргумента.",
                    "ЕСЛИ"));
            }

            return SelectBranch(arguments[0], arguments[1], arguments[2], "ЕСЛИ");
        }

        /// <summary>
        /// Выбирает одну из двух уже вычисленных веток по логическому условию.
        /// </summary>
        /// <param name="condition">Результат вычисления условия.</param>
        /// <param name="whenTrue">Значение для истинного условия.</param>
        /// <param name="whenFalse">Значение для ложного условия.</param>
        /// <param name="functionOrOperator">Имя функции или оператора для диагностики.</param>
        /// <returns>Выбранный результат или ошибка типа условия.</returns>
        public static FormulaResult SelectBranch(
            FormulaResult condition,
            FormulaResult whenTrue,
            FormulaResult whenFalse,
            string functionOrOperator)
        {
            if (TryGetCondition(condition, out var conditionValue) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Условный выбор ожидает логическое значение в условии.",
                    functionOrOperator));
            }

            return conditionValue ? whenTrue : whenFalse;
        }

        /// <summary>
        /// Проверяет и извлекает логическое значение условия.
        /// </summary>
        /// <param name="condition">Результат вычисления условия.</param>
        /// <param name="value">Логическое значение условия.</param>
        /// <returns>true, если условие имеет тип BOOL; иначе false.</returns>
        public static bool TryGetCondition(FormulaResult condition, out bool value)
        {
            value = false;
            if (condition.ValueType != SystemBaseType.BOOL || condition.Value is not bool conditionValue)
            {
                return false;
            }

            value = conditionValue;
            return true;
        }

        /// <summary>
        /// Создает ошибку условной формулы.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="functionOrOperator">Имя функции или оператора.</param>
        /// <returns>Ошибка формулы.</returns>
        private static FormulaError CreateError(
            FormulaErrorCode code,
            string message,
            string functionOrOperator)
        {
            return new FormulaError
            {
                Code = code,
                Message = message,
                FunctionOrOperator = functionOrOperator
            };
        }
    }
}
