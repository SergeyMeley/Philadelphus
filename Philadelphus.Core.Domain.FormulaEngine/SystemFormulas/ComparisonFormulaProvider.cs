using System.Globalization;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик системных формул сравнения и операторных псевдонимов.
    /// </summary>
    public sealed class ComparisonFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения формул сравнения этапа 1.
        /// </summary>
        /// <returns>Коллекция формул сравнения.</returns>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return CreateComparisonFormula("РАВНО", "=", "Проверяет равенство двух значений.", Equal);
            yield return CreateComparisonFormula("НЕ_РАВНО", "<>", "Проверяет неравенство двух значений.", NotEqual);
            yield return CreateComparisonFormula("БОЛЬШЕ", ">", "Проверяет, что первое значение больше второго.", Greater);
            yield return CreateComparisonFormula("МЕНЬШЕ", "<", "Проверяет, что первое значение меньше второго.", Less);
            yield return CreateComparisonFormula("БОЛЬШЕ_ИЛИ_РАВНО", ">=", "Проверяет, что первое значение больше второго или равно ему.", GreaterOrEqual);
            yield return CreateComparisonFormula("МЕНЬШЕ_ИЛИ_РАВНО", "<=", "Проверяет, что первое значение меньше второго или равно ему.", LessOrEqual);
        }

        /// <summary>
        /// Создает описание бинарной формулы сравнения.
        /// </summary>
        /// <param name="name">Имя формулы.</param>
        /// <param name="alias">Операторный псевдоним формулы.</param>
        /// <param name="description">Описание формулы.</param>
        /// <param name="operation">Операция сравнения.</param>
        /// <returns>Определение формулы сравнения.</returns>
        private static FormulaDefinition CreateComparisonFormula(
            string name,
            string alias,
            string description,
            Func<string, IReadOnlyList<FormulaResult>, FormulaResult> operation)
        {
            return new FormulaDefinition
            {
                Name = name,
                Aliases = [alias],
                Description = description,
                Category = "Сравнение",
                ResultType = SystemBaseType.BOOL,
                Examples = [CreateComparisonExample(alias)],
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "левое",
                        Description = "Первое сравниваемое значение."
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "правое",
                        Description = "Второе сравниваемое значение."
                    }
                ],
                Evaluator = (_, arguments) => EvaluateBinary(name, arguments, operation)
            };
        }

        /// <summary>
        /// Создает короткий пример сравнения для metadata редактора формул.
        /// </summary>
        private static string CreateComparisonExample(string alias)
        {
            return alias switch
            {
                "=" => "=10=(5*2)",
                "<>" => "=\"Стол\"<>\"Стул\"",
                ">" => "=3>2",
                "<" => "=2<3",
                ">=" => "=3>=3",
                "<=" => "=2<=3",
                _ => $"=1{alias}1"
            };
        }

        /// <summary>
        /// Проверяет общие требования к бинарному сравнению и выполняет его.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы формулы.</param>
        /// <param name="operation">Операция сравнения.</param>
        /// <returns>Результат сравнения.</returns>
        private static FormulaResult EvaluateBinary(
            string formulaName,
            IReadOnlyList<FormulaResult> arguments,
            Func<string, IReadOnlyList<FormulaResult>, FormulaResult> operation)
        {
            if (arguments.Count != 2)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    $"Формула '{formulaName}' ожидает два аргумента.",
                    formulaName));
            }

            return operation(formulaName, arguments);
        }

        /// <summary>
        /// Проверяет равенство двух совместимых значений.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult Equal(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            if (TryCompare(arguments[0], arguments[1], allowOrdering: false, out var comparison, out var errorMessage) == false)
            {
                return TypeMismatch(formulaName, errorMessage);
            }

            return Boolean(comparison == 0);
        }

        /// <summary>
        /// Проверяет неравенство двух совместимых значений.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult NotEqual(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            if (TryCompare(arguments[0], arguments[1], allowOrdering: false, out var comparison, out var errorMessage) == false)
            {
                return TypeMismatch(formulaName, errorMessage);
            }

            return Boolean(comparison != 0);
        }

        /// <summary>
        /// Проверяет, что первое значение больше второго.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult Greater(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            return OrderedComparison(formulaName, arguments, comparison => comparison > 0);
        }

        /// <summary>
        /// Проверяет, что первое значение меньше второго.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult Less(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            return OrderedComparison(formulaName, arguments, comparison => comparison < 0);
        }

        /// <summary>
        /// Проверяет, что первое значение больше второго или равно ему.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult GreaterOrEqual(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            return OrderedComparison(formulaName, arguments, comparison => comparison >= 0);
        }

        /// <summary>
        /// Проверяет, что первое значение меньше второго или равно ему.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <returns>Логический результат сравнения.</returns>
        private static FormulaResult LessOrEqual(string formulaName, IReadOnlyList<FormulaResult> arguments)
        {
            return OrderedComparison(formulaName, arguments, comparison => comparison <= 0);
        }

        /// <summary>
        /// Выполняет сравнение порядка для типов, где порядок определен.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы сравнения.</param>
        /// <param name="predicate">Проверка результата сравнения.</param>
        /// <returns>Логический результат или ошибка несовместимости типов.</returns>
        private static FormulaResult OrderedComparison(
            string formulaName,
            IReadOnlyList<FormulaResult> arguments,
            Func<int, bool> predicate)
        {
            if (TryCompare(arguments[0], arguments[1], allowOrdering: true, out var comparison, out var errorMessage) == false)
            {
                return TypeMismatch(formulaName, errorMessage);
            }

            return Boolean(predicate(comparison));
        }

        /// <summary>
        /// Пытается сравнить два значения с учетом строгих системных типов Formula Engine.
        /// </summary>
        /// <param name="left">Левое значение.</param>
        /// <param name="right">Правое значение.</param>
        /// <param name="allowOrdering">true, если требуется сравнение порядка.</param>
        /// <param name="comparison">Результат сравнения.</param>
        /// <param name="errorMessage">Сообщение об ошибке несовместимости типов.</param>
        /// <returns>true, если значения совместимы; иначе false.</returns>
        private static bool TryCompare(
            FormulaResult left,
            FormulaResult right,
            bool allowOrdering,
            out int comparison,
            out string errorMessage)
        {
            comparison = 0;
            errorMessage = string.Empty;

            if (TryGetNumber(left, out var leftNumber) && TryGetNumber(right, out var rightNumber))
            {
                comparison = leftNumber.CompareTo(rightNumber);
                return true;
            }

            if (left.ValueType == SystemBaseType.STRING && right.ValueType == SystemBaseType.STRING)
            {
                comparison = string.CompareOrdinal(left.Value as string, right.Value as string);
                return true;
            }

            if (allowOrdering == false
                && left.ValueType == SystemBaseType.BOOL
                && right.ValueType == SystemBaseType.BOOL
                && left.Value is bool leftBoolean
                && right.Value is bool rightBoolean)
            {
                comparison = leftBoolean.CompareTo(rightBoolean);
                return true;
            }

            errorMessage = allowOrdering
                ? "Операторы порядка поддерживают только числа и строки одного типа."
                : "Операторы равенства поддерживают только совместимые числовые значения, строки со строками и BOOL с BOOL.";
            return false;
        }

        /// <summary>
        /// Проверяет, является ли результат строгим числовым значением Formula Engine.
        /// </summary>
        /// <param name="argument">Аргумент формулы.</param>
        /// <param name="value">Числовое значение аргумента.</param>
        /// <returns>true, если аргумент является числом; иначе false.</returns>
        private static bool TryGetNumber(FormulaResult argument, out double value)
        {
            value = 0d;
            if (IsNumericType(argument.ValueType) == false || argument.Value is null or string)
            {
                return false;
            }

            try
            {
                value = Convert.ToDouble(argument.Value, CultureInfo.InvariantCulture);
                return double.IsNaN(value) == false && double.IsInfinity(value) == false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, относится ли системный тип к числовой группе.
        /// </summary>
        /// <param name="type">Системный тип значения.</param>
        /// <returns>true, если тип является числовым; иначе false.</returns>
        private static bool IsNumericType(SystemBaseType type)
        {
            return type is SystemBaseType.NUMERIC
                or SystemBaseType.INTEGER
                or SystemBaseType.FLOAT
                or SystemBaseType.MONEY;
        }

        /// <summary>
        /// Создает логический результат сравнения.
        /// </summary>
        /// <param name="value">Логическое значение результата.</param>
        /// <returns>Результат с системным типом BOOL.</returns>
        private static FormulaResult Boolean(bool value)
        {
            return FormulaResult.Success(value, SystemBaseType.BOOL);
        }

        /// <summary>
        /// Создает результат ошибки несовместимых типов.
        /// </summary>
        /// <param name="formulaName">Имя формулы.</param>
        /// <param name="message">Сообщение ошибки.</param>
        /// <returns>Неуспешный результат формулы.</returns>
        private static FormulaResult TypeMismatch(string formulaName, string message)
        {
            return FormulaResult.Failure(CreateError(FormulaErrorCode.TypeMismatch, message, formulaName));
        }

        /// <summary>
        /// Создает ошибку формулы сравнения.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="formulaName">Имя формулы.</param>
        /// <returns>Ошибка формулы.</returns>
        private static FormulaError CreateError(
            FormulaErrorCode code,
            string message,
            string formulaName)
        {
            return new FormulaError
            {
                Code = code,
                Message = message,
                FunctionOrOperator = formulaName
            };
        }
    }
}
