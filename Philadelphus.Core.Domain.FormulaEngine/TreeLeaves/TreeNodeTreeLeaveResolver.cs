using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.TreeLeaves
{
    /// <summary>
    /// Ищет значение атрибута среди дочерних листьев его узла типа данных.
    /// </summary>
    /// <remarks>
    /// Лист значения может принадлежать другому рабочему дереву, но всегда является непосредственным
    /// потомком узла <see cref="TreeNodeModel" />, выбранного как тип данных атрибута.
    /// </remarks>
    public sealed class TreeNodeTreeLeaveResolver : ITreeLeaveResolver
    {
        /// <summary>
        /// Узел типа данных, дочерние листья которого являются допустимыми значениями атрибута.
        /// </summary>
        private readonly TreeNodeModel _valueType;

        /// <summary>
        /// Создает resolver значений конкретного узла типа данных.
        /// </summary>
        /// <param name="valueType">Узел типа данных, среди дочерних листьев которого выполняется поиск.</param>
        /// <exception cref="ArgumentNullException">Узел типа данных не задан.</exception>
        public TreeNodeTreeLeaveResolver(TreeNodeModel valueType)
        {
            ArgumentNullException.ThrowIfNull(valueType);
            _valueType = valueType;
        }

        /// <summary>
        /// Ищет дочерний лист узла типа данных по UUID.
        /// </summary>
        /// <param name="uuid">UUID искомого листа.</param>
        /// <returns>Найденный лист или ошибка отсутствия листа.</returns>
        public TreeLeaveResolveResult ResolveByUuid(Guid uuid)
        {
            var treeLeave = _valueType.ChildLeaves.SingleOrDefault(x => x.Uuid == uuid);

            return treeLeave is null
                ? NotFound($"Лист типа данных '{_valueType.Name}' с UUID '{uuid}' не найден.", "[]")
                : TreeLeaveResolveResult.Resolved(treeLeave);
        }

        /// <summary>
        /// Ищет дочерний лист узла типа данных по наименованию без учета регистра.
        /// </summary>
        /// <param name="name">Наименование искомого листа.</param>
        /// <returns>Найденный лист или ошибка отсутствия листа.</returns>
        public TreeLeaveResolveResult ResolveByName(string name)
        {
            var treeLeave = _valueType.ChildLeaves.SingleOrDefault(
                x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            return treeLeave is null
                ? NotFound($"Лист типа данных '{_valueType.Name}' с наименованием '{name}' не найден.", "ЛИСТ")
                : TreeLeaveResolveResult.Resolved(treeLeave);
        }

        /// <summary>
        /// Возвращает ошибку зарезервированного, но пока не реализованного поиска по пользовательскому коду.
        /// </summary>
        /// <param name="userCode">Пользовательский код искомого листа.</param>
        /// <returns>Ошибка неподдерживаемого способа поиска.</returns>
        public TreeLeaveResolveResult ResolveByUserCode(string userCode)
        {
            return NotImplemented(
                $"Поиск листа по пользовательскому коду '{userCode}' пока не реализован.",
                "ЛИСТ");
        }

        /// <summary>
        /// Возвращает ошибку зарезервированного, но пока не реализованного поиска по псевдониму.
        /// </summary>
        /// <param name="alias">Псевдоним искомого листа.</param>
        /// <returns>Ошибка неподдерживаемого способа поиска.</returns>
        public TreeLeaveResolveResult ResolveByAlias(string alias)
        {
            return NotImplemented(
                $"Поиск листа по псевдониму '{alias}' пока не реализован.",
                "ЛИСТ");
        }

        /// <summary>
        /// Создает результат с ошибкой отсутствия листа.
        /// </summary>
        /// <param name="message">Описание ошибки.</param>
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
        /// Создает результат с ошибкой неподдерживаемого способа поиска.
        /// </summary>
        /// <param name="message">Описание ошибки.</param>
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
        /// Создает ошибку вычисления формулы для неуспешного поиска листа.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Описание ошибки.</param>
        /// <param name="functionOrOperator">Имя функции или оператора поиска.</param>
        /// <returns>Ошибка вычисления формулы.</returns>
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
