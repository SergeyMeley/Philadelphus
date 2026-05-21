using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Builders
{
    /// <summary>
    /// Строитель общих политик свойств.
    /// </summary>
    internal static class PropertiesPolicyBuilder
    {
        /// <summary>
        /// Создает политики рабочего дерева.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <returns>Созданный объект.</returns>
        public static IPropertiesPolicy<WorkingTreeModel> CreateWorkingTreeDefault(INotificationService notificationService)
        {
            return new CompositePropertiesPolicy<WorkingTreeModel>(notificationService, new IPropertiesRule<WorkingTreeModel>[]
            {
                new RequiredNamePropertiesRule<WorkingTreeModel>(notificationService),
                new ValidNamePropertiesRule<WorkingTreeModel>(notificationService, NameUniquenessStrategy.WorkingTree()),
            });
        }

        /// <summary>
        /// Создает политики корня дерева.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <returns>Созданный объект.</returns>
        public static IPropertiesPolicy<TreeRootModel> CreateTreeRootDefault(INotificationService notificationService)
        {
            return new CompositePropertiesPolicy<TreeRootModel>(notificationService, new IPropertiesRule<TreeRootModel>[]
            {
                new RequiredNamePropertiesRule<TreeRootModel>(notificationService),
                new ValidNamePropertiesRule<TreeRootModel>(notificationService, NameUniquenessStrategy.TreeRoot()),
                new SequencePropertiesRule<TreeRootModel>(notificationService, SequenceUniquenessStrategy.TreeRoot()),
                new CustomCodePropertiesRule<TreeRootModel>(notificationService, CustomCodeUniquenessStrategy.TreeRoot()),
            });
        }

        /// <summary>
        /// Создает политики узла дерева.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <returns>Созданный объект.</returns>
        public static IPropertiesPolicy<TreeNodeModel> CreateTreeNodeDefault(INotificationService notificationService)
        {
            return new CompositePropertiesPolicy<TreeNodeModel>(notificationService, new IPropertiesRule<TreeNodeModel>[]
            {
                new RequiredNamePropertiesRule<TreeNodeModel>(notificationService),
                new ValidNamePropertiesRule<TreeNodeModel>(notificationService, NameUniquenessStrategy.TreeNode()),
                new SequencePropertiesRule<TreeNodeModel>(notificationService, SequenceUniquenessStrategy.TreeNode()),
                new CustomCodePropertiesRule<TreeNodeModel>(notificationService, CustomCodeUniquenessStrategy.TreeNode()),
            });
        }

        /// <summary>
        /// Создает политики листа дерева.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <returns>Созданный объект.</returns>
        public static IPropertiesPolicy<TreeLeaveModel> CreateTreeLeaveDefault(INotificationService notificationService)
        {
            return new CompositePropertiesPolicy<TreeLeaveModel>(notificationService, new IPropertiesRule<TreeLeaveModel>[]
            {
                // BOOL-листья являются фиксированным справочником системных значений.
                // Проверяем их первыми, чтобы любые изменения блокировались до общих правил листа.
                new SystemBaseBoolTreeLeaveReadOnlyPropertiesRule(notificationService),
                new RequiredNamePropertiesRule<TreeLeaveModel>(notificationService),
                new ValidNamePropertiesRule<TreeLeaveModel>(notificationService, NameUniquenessStrategy.TreeLeave()),
                new SequencePropertiesRule<TreeLeaveModel>(notificationService, SequenceUniquenessStrategy.TreeLeave()),
                new CustomCodePropertiesRule<TreeLeaveModel>(notificationService, CustomCodeUniquenessStrategy.TreeLeave()),
            });
        }
    }
}
