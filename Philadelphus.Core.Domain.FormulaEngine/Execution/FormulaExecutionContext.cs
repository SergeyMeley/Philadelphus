using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.FormulaEngine.Execution
{
    /// <summary>
    /// Данные времени выполнения, доступные при вычислении формулы.
    /// </summary>
    public sealed class FormulaExecutionContext
    {
        /// <summary>
        /// Рабочее дерево, в контексте которого вычисляется формула.
        /// </summary>
        public WorkingTreeModel? WorkingTree { get; init; }

        /// <summary>
        /// Сервис уведомлений текущего приложения.
        /// </summary>
        public INotificationService? NotificationService { get; init; }

        /// <summary>
        /// Дополнительные данные контекста для будущих сценариев.
        /// </summary>
        public IDictionary<string, object?> Items { get; init; } = new Dictionary<string, object?>();
    }
}
