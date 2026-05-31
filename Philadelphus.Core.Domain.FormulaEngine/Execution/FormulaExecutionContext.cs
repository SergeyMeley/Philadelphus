using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
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
        /// Сервис поиска листьев для формул по UUID, наименованию и будущим пользовательским идентификаторам.
        /// </summary>
        public ITreeLeaveResolver? TreeLeaveResolver { get; init; }

        /// <summary>
        /// Системное рабочее дерево, из которого берутся предопределенные системные значения.
        /// </summary>
        public WorkingTreeModel? SystemBaseWorkingTree { get; init; }

        /// <summary>
        /// Доменный сервис репозитория, через который формулы создают недостающие листья результата.
        /// </summary>
        public IPhiladelphusRepositoryService? RepositoryService { get; init; }

        /// <summary>
        /// Сервис уведомлений текущего приложения.
        /// </summary>
        public INotificationService? NotificationService { get; init; }

        /// <summary>
        /// Приемник диагностики Formula Engine для уведомлений и журнала приложения.
        /// </summary>
        public IFormulaDiagnosticsReporter? DiagnosticsReporter { get; init; }

        /// <summary>
        /// Дополнительные данные контекста для будущих сценариев.
        /// </summary>
        public IDictionary<string, object?> Items { get; init; } = new Dictionary<string, object?>();
    }
}
