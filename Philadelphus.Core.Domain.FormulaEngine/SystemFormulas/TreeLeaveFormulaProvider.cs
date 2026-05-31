using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using System.Globalization;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик системной формулы поиска листа рабочего дерева.
    /// </summary>
    public sealed class TreeLeaveFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения формул поиска листьев.
        /// </summary>
        /// <returns>Коллекция формул поиска листьев.</returns>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "ЛИСТ",
                Description = "Ищет лист рабочего дерева по UUID, наименованию или зарезервированным идентификаторам.",
                Category = "Листья дерева",
                Examples = ["=ЛИСТ(0;\"9769db19-89a2-49ed-a3fd-f1e539a652b1\")", "=[9769db19-89a2-49ed-a3fd-f1e539a652b1]"],
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "метод",
                        Description = "Способ поиска: 0 - UUID, 1 - наименование, 2 - пользовательский код, 3 - псевдоним.",
                        ExpectedType = SystemBaseType.INTEGER
                    },
                    new FormulaArgumentDefinition
                    {
                        Name = "значение",
                        Description = "Значение идентификатора листа.",
                        ExpectedType = SystemBaseType.STRING
                    }
                ],
                Evaluator = (context, arguments) => Evaluate(context, arguments)
            };
        }

        /// <summary>
        /// Вычисляет формулу ЛИСТ по выбранному способу поиска.
        /// </summary>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <param name="arguments">Аргументы формулы.</param>
        /// <returns>Найденный лист или ошибка поиска.</returns>
        private static FormulaResult Evaluate(
            FormulaExecutionContext context,
            IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count != 2)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentCount,
                    "Формула 'ЛИСТ' ожидает два аргумента.",
                    "ЛИСТ"));
            }

            if (TryGetMethod(arguments[0], out var method) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Первый аргумент формулы 'ЛИСТ' должен быть числовым методом поиска.",
                    "ЛИСТ"));
            }

            if (TryGetSearchValue(arguments[1], out var value) == false)
            {
                return FormulaResult.Failure(CreateError(
                    FormulaErrorCode.TypeMismatch,
                    "Второй аргумент формулы 'ЛИСТ' должен быть строковым значением поиска.",
                    "ЛИСТ"));
            }

            var resolver = context.TreeLeaveResolver ?? new WorkingTreeTreeLeaveResolver(context.WorkingTree);
            var resolveResult = method switch
            {
                0 => ResolveByUuid(resolver, value),
                1 => resolver.ResolveByName(value),
                2 => resolver.ResolveByUserCode(value),
                3 => resolver.ResolveByAlias(value),
                _ => TreeLeaveResolveResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentValue,
                    $"Метод поиска листа '{method}' не поддерживается.",
                    "ЛИСТ"))
            };

            return resolveResult.IsResolved
                ? FormulaResult.FromTreeLeave(resolveResult.TreeLeave!)
                : FormulaResult.Failure(resolveResult.Error!);
        }

        /// <summary>
        /// Извлекает числовой метод поиска из первого аргумента.
        /// </summary>
        /// <param name="argument">Аргумент метода поиска.</param>
        /// <param name="method">Номер метода поиска.</param>
        /// <returns>true, если метод удалось получить; иначе false.</returns>
        private static bool TryGetMethod(FormulaResult argument, out int method)
        {
            method = 0;
            if (argument.Value is string || argument.Value is null)
            {
                return false;
            }

            switch (argument.ValueType)
            {
                case SystemBaseType.INTEGER:
                case SystemBaseType.NUMERIC:
                case SystemBaseType.FLOAT:
                    var value = Convert.ToDouble(argument.Value, CultureInfo.InvariantCulture);
                    if (double.IsNaN(value)
                        || double.IsInfinity(value)
                        || Math.Truncate(value) != value
                        || value < int.MinValue
                        || value > int.MaxValue)
                    {
                        return false;
                    }

                    method = Convert.ToInt32(value);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Извлекает строковое значение поиска из второго аргумента.
        /// </summary>
        /// <param name="argument">Аргумент значения поиска.</param>
        /// <param name="value">Строковое значение поиска.</param>
        /// <returns>true, если значение является строкой; иначе false.</returns>
        private static bool TryGetSearchValue(FormulaResult argument, out string value)
        {
            value = string.Empty;
            if (argument.ValueType != SystemBaseType.STRING || argument.Value is not string stringValue)
            {
                return false;
            }

            value = stringValue;
            return true;
        }

        /// <summary>
        /// Ищет лист по UUID, предварительно проверяя строковый формат идентификатора.
        /// </summary>
        /// <param name="resolver">Resolver листьев рабочего дерева.</param>
        /// <param name="value">Строковое значение UUID.</param>
        /// <returns>Результат поиска листа.</returns>
        private static TreeLeaveResolveResult ResolveByUuid(ITreeLeaveResolver resolver, string value)
        {
            return Guid.TryParse(value, out var uuid)
                ? resolver.ResolveByUuid(uuid)
                : TreeLeaveResolveResult.Failure(CreateError(
                    FormulaErrorCode.InvalidArgumentValue,
                    $"Значение '{value}' не является корректным UUID.",
                    "ЛИСТ"));
        }

        /// <summary>
        /// Создает ошибку формулы ЛИСТ.
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
