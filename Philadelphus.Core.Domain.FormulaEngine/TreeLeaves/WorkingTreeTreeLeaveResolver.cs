using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.TreeLeaves
{
    /// <summary>
    /// Ищет листья формул в содержимом конкретного рабочего дерева.
    /// </summary>
    public sealed class WorkingTreeTreeLeaveResolver : ITreeLeaveResolver
    {
        /// <summary>
        /// Рабочее дерево, используемое как источник листьев.
        /// </summary>
        private readonly WorkingTreeModel? _workingTree;

        /// <summary>
        /// Инициализирует resolver листьев по рабочему дереву.
        /// </summary>
        /// <param name="workingTree">Рабочее дерево, в котором выполняется поиск.</param>
        public WorkingTreeTreeLeaveResolver(WorkingTreeModel? workingTree)
        {
            _workingTree = workingTree;
        }

        /// <summary>
        /// Ищет лист по UUID.
        /// </summary>
        /// <param name="uuid">UUID листа рабочего дерева.</param>
        /// <returns>Результат поиска листа.</returns>
        public TreeLeaveResolveResult ResolveByUuid(Guid uuid)
        {
            var treeLeave = _workingTree?.ContentLeaves
                .SingleOrDefault(x => x.Uuid == uuid);

            return treeLeave is null
                ? NotFound($"Лист дерева '{uuid}' не найден.", "[]")
                : TreeLeaveResolveResult.Resolved(treeLeave);
        }

        /// <summary>
        /// Ищет лист по наименованию.
        /// </summary>
        /// <param name="name">Наименование листа рабочего дерева.</param>
        /// <returns>Результат поиска листа.</returns>
        public TreeLeaveResolveResult ResolveByName(string name)
        {
            var treeLeave = _workingTree?.ContentLeaves
                .SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            return treeLeave is null
                ? NotFound($"Лист дерева с наименованием '{name}' не найден.", "ЛИСТ")
                : TreeLeaveResolveResult.Resolved(treeLeave);
        }

        /// <summary>
        /// Возвращает зарезервированную ошибку для поиска по пользовательскому коду.
        /// </summary>
        /// <param name="userCode">Пользовательский код листа.</param>
        /// <returns>Ошибка не реализованного поиска.</returns>
        public TreeLeaveResolveResult ResolveByUserCode(string userCode)
        {
            return NotImplemented(
                $"Поиск листа по пользовательскому коду '{userCode}' пока не реализован.",
                "ЛИСТ");
        }

        /// <summary>
        /// Возвращает зарезервированную ошибку для поиска по псевдониму.
        /// </summary>
        /// <param name="alias">Псевдоним листа.</param>
        /// <returns>Ошибка не реализованного поиска.</returns>
        public TreeLeaveResolveResult ResolveByAlias(string alias)
        {
            return NotImplemented(
                $"Поиск листа по псевдониму '{alias}' пока не реализован.",
                "ЛИСТ");
        }

        /// <summary>
        /// Создает ошибку отсутствующего листа.
        /// </summary>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="functionOrOperator">Имя функции или оператора поиска.</param>
        /// <returns>Неуспешный результат поиска.</returns>
        private static TreeLeaveResolveResult NotFound(string message, string functionOrOperator)
        {
            return TreeLeaveResolveResult.Failure(CreateError(
                FormulaErrorCode.TreeLeaveNotFound,
                message,
                functionOrOperator));
        }

        /// <summary>
        /// Создает ошибку зарезервированного способа поиска.
        /// </summary>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="functionOrOperator">Имя функции или оператора поиска.</param>
        /// <returns>Неуспешный результат поиска.</returns>
        private static TreeLeaveResolveResult NotImplemented(string message, string functionOrOperator)
        {
            return TreeLeaveResolveResult.Failure(CreateError(
                FormulaErrorCode.NotImplemented,
                message,
                functionOrOperator));
        }

        /// <summary>
        /// Создает ошибку поиска листа.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="functionOrOperator">Имя функции или оператора поиска.</param>
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
