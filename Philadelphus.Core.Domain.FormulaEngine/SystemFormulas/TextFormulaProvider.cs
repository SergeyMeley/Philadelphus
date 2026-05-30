using System.Globalization;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.SystemFormulas
{
    /// <summary>
    /// Поставщик системных текстовых формул и операторных псевдонимов.
    /// </summary>
    public sealed class TextFormulaProvider : IFormulaProvider
    {
        /// <summary>
        /// Возвращает определения текстовых формул этапа 1.
        /// </summary>
        /// <returns>Коллекция текстовых формул.</returns>
        public IEnumerable<FormulaDefinition> GetFormulas()
        {
            yield return new FormulaDefinition
            {
                Name = "СЦЕПИТЬ",
                Aliases = ["&"],
                Description = "Объединяет значения в строку.",
                Arguments =
                [
                    new FormulaArgumentDefinition
                    {
                        Name = "значение",
                        Description = "Значение для добавления к строке.",
                        ExpectedType = SystemBaseType.STRING
                    }
                ],
                Evaluator = (_, arguments) => Concatenate(arguments)
            };
        }

        /// <summary>
        /// Объединяет аргументы в строковый результат.
        /// </summary>
        /// <param name="arguments">Аргументы формулы.</param>
        /// <returns>Строковый результат или ошибка несовместимого типа.</returns>
        private static FormulaResult Concatenate(IReadOnlyList<FormulaResult> arguments)
        {
            if (arguments.Count == 0)
            {
                return FormulaResult.Success(string.Empty, SystemBaseType.STRING);
            }

            var parts = new List<string>(arguments.Count);
            for (var index = 0; index < arguments.Count; index++)
            {
                if (TryFormatText(arguments[index], out var text) == false)
                {
                    return FormulaResult.Failure(CreateError(
                        FormulaErrorCode.TypeMismatch,
                        $"Аргумент {index + 1} формулы 'СЦЕПИТЬ' нельзя преобразовать в текст.",
                        "СЦЕПИТЬ"));
                }

                parts.Add(text);
            }

            return FormulaResult.Success(string.Concat(parts), SystemBaseType.STRING);
        }

        /// <summary>
        /// Преобразует уже типизированный результат формулы в текст для конкатенации.
        /// </summary>
        /// <param name="argument">Аргумент формулы.</param>
        /// <param name="text">Текстовое представление аргумента.</param>
        /// <returns>true, если тип аргумента поддержан; иначе false.</returns>
        private static bool TryFormatText(FormulaResult argument, out string text)
        {
            text = string.Empty;
            if (argument.TreeLeave is not null || argument.Value is null)
            {
                return false;
            }

            switch (argument.ValueType)
            {
                case SystemBaseType.STRING:
                    text = argument.Value as string ?? string.Empty;
                    return true;
                case SystemBaseType.NUMERIC:
                case SystemBaseType.INTEGER:
                case SystemBaseType.FLOAT:
                case SystemBaseType.MONEY:
                    if (argument.Value is string)
                    {
                        return false;
                    }

                    text = Convert.ToString(argument.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    return true;
                case SystemBaseType.BOOL:
                    if (argument.Value is not bool boolValue)
                    {
                        return false;
                    }

                    text = boolValue ? "Истина" : "Ложь";
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Создает ошибку текстовой формулы.
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
