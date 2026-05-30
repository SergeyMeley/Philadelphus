using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Execution
{
    /// <summary>
    /// Тонкая оболочка результата формулы поверх существующих доменных типов значений.
    /// </summary>
    public sealed class FormulaResult
    {
        /// <summary>
        /// Инициализирует общий результат вычисления формулы.
        /// </summary>
        /// <param name="value">Значение результата.</param>
        /// <param name="valueType">Системный тип результата.</param>
        /// <param name="treeLeave">Лист дерева, если результатом является лист.</param>
        /// <param name="error">Ошибка вычисления, если результат неуспешный.</param>
        private FormulaResult(
            object? value,
            SystemBaseType valueType,
            TreeLeaveModel? treeLeave,
            FormulaError? error)
        {
            Value = value;
            ValueType = valueType;
            TreeLeave = treeLeave;
            Error = error;
        }

        /// <summary>
        /// Признак успешного вычисления.
        /// </summary>
        public bool IsSuccess => Error is null;

        /// <summary>
        /// Значение результата.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Системный тип результата.
        /// </summary>
        public SystemBaseType ValueType { get; }

        /// <summary>
        /// Лист дерева, если результатом формулы является ссылка на лист.
        /// </summary>
        public TreeLeaveModel? TreeLeave { get; }

        /// <summary>
        /// Ошибка вычисления, если формула завершилась неуспешно.
        /// </summary>
        public FormulaError? Error { get; }

        /// <summary>
        /// Создает успешный результат формулы.
        /// </summary>
        /// <param name="value">Значение результата.</param>
        /// <param name="valueType">Системный тип результата.</param>
        /// <param name="treeLeave">Лист дерева, если результат связан с листом.</param>
        /// <returns>Успешный результат формулы.</returns>
        public static FormulaResult Success(
            object? value,
            SystemBaseType valueType,
            TreeLeaveModel? treeLeave = null)
        {
            return new FormulaResult(value, valueType, treeLeave, null);
        }

        /// <summary>
        /// Создает результат, содержащий сам лист дерева.
        /// </summary>
        /// <param name="treeLeave">Лист дерева.</param>
        /// <returns>Успешный результат со ссылкой на лист.</returns>
        public static FormulaResult FromTreeLeave(TreeLeaveModel treeLeave)
        {
            ArgumentNullException.ThrowIfNull(treeLeave);

            return new FormulaResult(treeLeave, treeLeave.SystemBaseType, treeLeave, null);
        }

        /// <summary>
        /// Создает результат с ошибкой.
        /// </summary>
        /// <param name="error">Ошибка формулы.</param>
        /// <returns>Неуспешный результат формулы.</returns>
        public static FormulaResult Failure(FormulaError error)
        {
            ArgumentNullException.ThrowIfNull(error);

            return new FormulaResult(null, SystemBaseType.USER_DEFINED, null, error);
        }
    }
}
