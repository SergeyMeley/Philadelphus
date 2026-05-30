using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
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
                        Description = "Значение, возвращаемое при истинном условии.",
                        IsRequired = false,
                        DefaultValue = true
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "еслиЛожь",
                        Description = "Значение, возвращаемое при ложном условии.",
                        IsRequired = false,
                        DefaultValue = false
                    }
                ],
                Evaluator = (context, arguments) => EvaluateIf(context, arguments)
            };
        }

        /// <summary>
        /// Вычисляет именованную формулу ЕСЛИ по уже вычисленным аргументам.
        /// </summary>
        /// <param name="context">Контекст вычисления, содержащий системное рабочее дерево.</param>
        /// <param name="arguments">Аргументы формулы ЕСЛИ.</param>
        /// <returns>Выбранный результат или ошибка условия.</returns>
        private static FormulaResult EvaluateIf(FormulaExecutionContext context, IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count is < 1 or > 3)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'ЕСЛИ' ожидает от одного до трех аргументов.",
                    "ЕСЛИ"));
            }

            if (arguments.Count == 3)
            {
                return SelectBranch(arguments[0], arguments[1], arguments[2], "ЕСЛИ");
            }

            if (TryGetCondition(arguments[0], out var conditionValue) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Условный выбор ожидает логическое значение в условии.",
                    "ЕСЛИ"));
            }

            if (conditionValue)
            {
                return arguments.Count > 1
                    ? arguments[1]
                    : ResolveSystemBaseBoolean(context, true);
            }

            return ResolveSystemBaseBoolean(context, false);
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
        /// Возвращает предопределенный системный лист BOOL из системного рабочего дерева.
        /// </summary>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <param name="value">Требуемое логическое значение.</param>
        /// <returns>Результат на основе системного листа или ошибка отсутствия системного значения.</returns>
        private static FormulaResult ResolveSystemBaseBoolean(FormulaExecutionContext context, bool value)
        {
            var treeLeave = context.SystemBaseWorkingTree?.ContentLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .SingleOrDefault(x =>
                    x.SystemBaseType == SystemBaseType.BOOL
                    && x.TypedValue is bool boolValue
                    && boolValue == value);

            if (treeLeave is null)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TreeLeaveNotFound,
                    $"Системное логическое значение '{(value ? "Истина" : "Ложь")}' не найдено.",
                    "ЕСЛИ"));
            }

            return FormulaResult.FromSystemBaseTreeLeave(treeLeave);
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
