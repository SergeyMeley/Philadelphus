using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.Helpers;

namespace Philadelphus.Core.Domain.FormulaEngine.Execution
{
    /// <summary>
    /// Материализует успешные результаты формул в существующие или новые доменные листья.
    /// </summary>
    public static class FormulaResultMaterializer
    {
        /// <summary>
        /// Возвращает результат, связанный с листом системного рабочего дерева.
        /// </summary>
        /// <param name="result">Исходный результат формулы.</param>
        /// <param name="context">Контекст вычисления с системным деревом и сервисом создания листьев.</param>
        /// <param name="span">Диапазон выражения, для которого создается диагностическая ошибка.</param>
        /// <param name="functionOrOperator">Имя функции или оператора для диагностики.</param>
        /// <returns>Результат с доменным листом или ошибка материализации.</returns>
        public static FormulaResult Materialize(
            FormulaResult result,
            FormulaExecutionContext context,
            FormulaTextSpan? span = null,
            string? functionOrOperator = null)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(context);

            if (result.IsSuccess == false
                || result.TreeLeave is not null
                || result.TreeLeaves is not null)
            {
                return result;
            }

            if (SystemBaseStringValueValidator.TryFormat(result.ValueType, result.Value, out var stringValue) == false
                || stringValue is null)
            {
                return Failure(
                    FormulaErrorCode.TypeMismatch,
                    $"Результат типа '{result.ValueType}' нельзя записать в системный лист.",
                    span,
                    functionOrOperator);
            }

            var node = ResolveSystemBaseNode(context, result.ValueType);
            if (node is null)
            {
                return Failure(
                    FormulaErrorCode.TreeLeaveNotFound,
                    $"Системный узел типа '{result.ValueType}' не найден.",
                    span,
                    functionOrOperator);
            }

            var existing = FindExistingLeave(node, stringValue);
            if (existing is not null)
            {
                return FormulaResult.FromSystemBaseTreeLeave(existing);
            }

            if (result.ValueType == SystemBaseType.BOOL)
            {
                return Failure(
                    FormulaErrorCode.TreeLeaveNotFound,
                    $"Предопределенный системный лист BOOL со значением '{stringValue}' не найден.",
                    span,
                    functionOrOperator);
            }

            if (context.RepositoryService is null)
            {
                return Failure(
                    FormulaErrorCode.NotImplemented,
                    "В контексте вычисления нет доменного сервиса для создания листа результата формулы.",
                    span,
                    functionOrOperator);
            }

            var created = context.RepositoryService.CreateTreeLeave(
                node,
                needAutoName: false,
                withoutInfoNotifications: true);

            if (created is not SystemBaseTreeLeaveModel systemBaseCreated)
            {
                return Failure(
                    FormulaErrorCode.TypeMismatch,
                    $"Доменный сервис не создал системный лист типа '{result.ValueType}'.",
                    span,
                    functionOrOperator);
            }

            systemBaseCreated.StringValue = stringValue;
            return FormulaResult.FromSystemBaseTreeLeave(systemBaseCreated);
        }

        /// <summary>
        /// Находит системный узел, соответствующий типу результата формулы.
        /// </summary>
        /// <param name="context">Контекст вычисления формулы.</param>
        /// <param name="type">Системный тип результата.</param>
        /// <returns>Системный узел или null, если дерево не содержит такой тип.</returns>
        private static SystemBaseTreeNodeModel? ResolveSystemBaseNode(
            FormulaExecutionContext context,
            SystemBaseType type)
        {
            return context.SystemBaseWorkingTree?.ContentNodes
                .OfType<SystemBaseTreeNodeModel>()
                .SingleOrDefault(x => x.SystemBaseType == type);
        }

        /// <summary>
        /// Ищет существующий системный лист по родительскому узлу и строковому значению.
        /// </summary>
        /// <param name="node">Системный узел типа результата.</param>
        /// <param name="stringValue">Строковое значение результата.</param>
        /// <returns>Найденный системный лист или null.</returns>
        private static SystemBaseTreeLeaveModel? FindExistingLeave(
            SystemBaseTreeNodeModel node,
            string stringValue)
        {
            return node.OwningWorkingTree.ContentLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x =>
                    x.ParentNode?.Uuid == node.Uuid
                    && string.Equals(x.StringValue, stringValue, StringComparison.Ordinal));
        }

        /// <summary>
        /// Создает ошибку материализации результата формулы.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="span">Диапазон выражения.</param>
        /// <param name="functionOrOperator">Имя функции или оператора.</param>
        /// <returns>Негативный результат формулы.</returns>
        private static FormulaResult Failure(
            FormulaErrorCode code,
            string message,
            FormulaTextSpan? span,
            string? functionOrOperator)
        {
            return FormulaResult.Failure(new FormulaError
            {
                Code = code,
                Message = message,
                Span = span,
                FunctionOrOperator = functionOrOperator
            });
        }
    }
}
