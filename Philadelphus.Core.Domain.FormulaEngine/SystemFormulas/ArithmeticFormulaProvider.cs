using System.Globalization;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик базовых арифметических формул и операторных псевдонимов.
    /// </summary>
    public sealed class ArithmeticFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения арифметических формул этапа 1.
        /// </summary>
        /// <returns>Коллекция арифметических формул.</returns>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return CreateSumFormula();
            yield return CreateBinaryFormula("РАЗНОСТЬ", "-", "Вычитает второе числовое значение из первого.", Subtract);
            yield return CreateProductFormula();
            yield return CreateBinaryFormula("ЧАСТНОЕ", "/", "Делит первое числовое значение на второе.", Divide);
            yield return CreateBinaryFormula("СТЕПЕНЬ", "^", "Возводит первое числовое значение в степень второго.", Power);
        }

        /// <summary>
        /// Создает описание формулы СУММ и операторного псевдонима +.
        /// </summary>
        /// <returns>Определение формулы СУММ.</returns>
        private static FormulaDefinition CreateSumFormula()
        {
            return new FormulaDefinition
            {
                Name = "СУММ",
                Aliases = ["+"],
                Description = "Складывает числовые значения.",
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "значение",
                        Description = "Числовое значение для сложения.",
                        ExpectedType = SystemBaseType.NUMERIC
                    }
                ],
                Evaluator = (_, arguments) => Sum(arguments)
            };
        }

        /// <summary>
        /// Создает описание формулы ПРОИЗВ и операторного псевдонима *.
        /// </summary>
        /// <returns>Определение формулы ПРОИЗВ.</returns>
        private static FormulaDefinition CreateProductFormula()
        {
            return new FormulaDefinition
            {
                Name = "ПРОИЗВ",
                Aliases = ["*"],
                Description = "Умножает числовые значения.",
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "значение",
                        Description = "Числовое значение для умножения.",
                        ExpectedType = SystemBaseType.NUMERIC
                    }
                ],
                Evaluator = (_, arguments) => Product(arguments)
            };
        }

        /// <summary>
        /// Создает описание бинарной арифметической формулы.
        /// </summary>
        /// <param name="name">Имя формулы.</param>
        /// <param name="alias">Операторный псевдоним формулы.</param>
        /// <param name="description">Описание формулы.</param>
        /// <param name="operation">Арифметическая операция.</param>
        /// <returns>Определение формулы.</returns>
        private static FormulaDefinition CreateBinaryFormula(
            string name,
            string alias,
            string description,
            Func<IReadOnlyList<FormulaResult>, FormulaResult> operation)
        {
            return new FormulaDefinition
            {
                Name = name,
                Aliases = [alias],
                Description = description,
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "левое",
                        Description = "Первое числовое значение.",
                        ExpectedType = SystemBaseType.NUMERIC
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "правое",
                        Description = "Второе числовое значение.",
                        ExpectedType = SystemBaseType.NUMERIC
                    }
                ],
                Evaluator = (_, arguments) => EvaluateBinary(name, arguments, operation)
            };
        }

        /// <summary>
        /// Проверяет общие требования к бинарной операции и выполняет ее.
        /// </summary>
        /// <param name="formulaName">Имя формулы для диагностики ошибок.</param>
        /// <param name="arguments">Аргументы формулы.</param>
        /// <param name="operation">Арифметическая операция.</param>
        /// <returns>Результат вычисления операции.</returns>
        private static FormulaResult EvaluateBinary(
            string formulaName,
            IReadOnlyList<FormulaResult> arguments,
            Func<IReadOnlyList<FormulaResult>, FormulaResult> operation)
        {
            if (arguments.Count != 2)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    $"Формула '{formulaName}' ожидает два аргумента.",
                    formulaName));
            }

            for (var index = 0; index < arguments.Count; index++)
            {
                if (TryGetNumber(arguments[index], out _) == false)
                {
                    return FormulaResult.Failure(CreateError(
                        FormulaErrorCode.TypeMismatch,
                        $"Аргумент {index + 1} формулы '{formulaName}' должен быть числовым.",
                        formulaName));
                }
            }

            return operation(arguments);
        }

        /// <summary>
        /// Складывает один или несколько числовых аргументов.
        /// </summary>
        /// <param name="arguments">Аргументы формулы СУММ.</param>
        /// <returns>Результат сложения или ошибка аргументов.</returns>
        private static FormulaResult Sum(IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count == 0)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'СУММ' ожидает минимум один аргумент.",
                    "СУММ"));
            }

            var result = 0d;
            for (var index = 0; index < arguments.Count; index++)
            {
                if (TryGetNumber(arguments[index], out var value) == false)
                {
                    return FormulaResult.Failure(CreateError(
                        FormulaErrorCode.TypeMismatch,
                        $"Аргумент {index + 1} формулы 'СУММ' должен быть числовым.",
                        "СУММ"));
                }

                result += value;
            }

            return Numeric(result);
        }

        /// <summary>
        /// Вычитает второй числовой аргумент из первого.
        /// </summary>
        /// <param name="arguments">Аргументы операции.</param>
        /// <returns>Результат вычитания.</returns>
        private static FormulaResult Subtract(IReadOnlyList<FormulaResult> arguments)
        {
            var left = GetNumber(arguments[0]);
            var right = GetNumber(arguments[1]);

            return Numeric(left - right);
        }

        /// <summary>
        /// Умножает один или несколько числовых аргументов.
        /// </summary>
        /// <param name="arguments">Аргументы формулы ПРОИЗВ.</param>
        /// <returns>Результат умножения или ошибка аргументов.</returns>
        private static FormulaResult Product(IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count == 0)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'ПРОИЗВ' ожидает минимум один аргумент.",
                    "ПРОИЗВ"));
            }

            var result = 1d;
            for (var index = 0; index < arguments.Count; index++)
            {
                if (TryGetNumber(arguments[index], out var value) == false)
                {
                    return FormulaResult.Failure(CreateError(
                        FormulaErrorCode.TypeMismatch,
                        $"Аргумент {index + 1} формулы 'ПРОИЗВ' должен быть числовым.",
                        "ПРОИЗВ"));
                }

                result *= value;
            }

            return Numeric(result);
        }

        /// <summary>
        /// Делит первый числовой аргумент на второй.
        /// </summary>
        /// <param name="arguments">Аргументы операции.</param>
        /// <returns>Результат деления или ошибка деления на ноль.</returns>
        private static FormulaResult Divide(IReadOnlyList<FormulaResult> arguments)
        {
            var left = GetNumber(arguments[0]);
            var right = GetNumber(arguments[1]);

            if (right == 0d)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.DivZero,
                    "Деление на ноль невозможно.",
                    "ЧАСТНОЕ"));
            }

            return Numeric(left / right);
        }

        /// <summary>
        /// Возводит первый числовой аргумент в степень второго.
        /// </summary>
        /// <param name="arguments">Аргументы операции.</param>
        /// <returns>Результат возведения в степень.</returns>
        private static FormulaResult Power(IReadOnlyList<FormulaResult> arguments)
        {
            var left = GetNumber(arguments[0]);
            var right = GetNumber(arguments[1]);

            return Numeric(Math.Pow(left, right));
        }

        /// <summary>
        /// Создает числовой результат формулы.
        /// </summary>
        /// <param name="value">Числовое значение результата.</param>
        /// <returns>Результат с системным типом NUMERIC.</returns>
        private static FormulaResult Numeric(double value)
        {
            return FormulaResult.Success(value, SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Возвращает числовое значение аргумента после предварительной проверки типа.
        /// </summary>
        /// <param name="argument">Аргумент формулы.</param>
        /// <returns>Числовое значение аргумента.</returns>
        private static double GetNumber(FormulaResult argument)
        {
            TryGetNumber(argument, out var value);
            return value;
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
        /// Создает ошибку арифметической формулы.
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
