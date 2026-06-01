using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.TreeLeaves
{
    /// <summary>
    /// Результат поиска листа рабочего дерева для вычисления формулы.
    /// </summary>
    public sealed class TreeLeaveResolveResult
    {
        /// <summary>
        /// Инициализирует результат поиска листа.
        /// </summary>
        /// <param name="treeLeave">Найденный лист рабочего дерева.</param>
        /// <param name="error">Ошибка поиска.</param>
        private TreeLeaveResolveResult(TreeLeaveModel? treeLeave, FormulaError? error)
        {
            TreeLeave = treeLeave;
            Error = error;
        }

        /// <summary>
        /// Признак успешного поиска листа.
        /// </summary>
        public bool IsResolved => TreeLeave is not null && Error is null;

        /// <summary>
        /// Найденный лист рабочего дерева.
        /// </summary>
        public TreeLeaveModel? TreeLeave { get; }

        /// <summary>
        /// Ошибка поиска, если лист не найден или способ поиска еще не реализован.
        /// </summary>
        public FormulaError? Error { get; }

        /// <summary>
        /// Создает успешный результат поиска листа.
        /// </summary>
        /// <param name="treeLeave">Найденный лист рабочего дерева.</param>
        /// <returns>Успешный результат поиска.</returns>
        public static TreeLeaveResolveResult Resolved(TreeLeaveModel treeLeave)
        {
            ArgumentNullException.ThrowIfNull(treeLeave);

            return new TreeLeaveResolveResult(treeLeave, null);
        }

        /// <summary>
        /// Создает результат поиска с ошибкой.
        /// </summary>
        /// <param name="error">Ошибка поиска листа.</param>
        /// <returns>Неуспешный результат поиска.</returns>
        public static TreeLeaveResolveResult Failure(FormulaError error)
        {
            ArgumentNullException.ThrowIfNull(error);

            return new TreeLeaveResolveResult(null, error);
        }
    }
}
