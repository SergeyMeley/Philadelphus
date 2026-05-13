using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Доменная модель системного листа рабочего дерева.
    /// </summary>
    public class SystemBaseTreeLeaveModel : TreeLeaveModel
    {
        private string _stringValue;

        /// <summary>
        /// Тип.
        /// </summary>
        public override SystemBaseType SystemBaseType { get; }
        public string StringValue 
        { 
            get
            {
                return _stringValue; 
            }
            set
            {
                _stringValue = value;
                Name = value;
                Description = value;
            }
        }
        internal SystemBaseTreeLeaveModel(
            Guid uuid, 
            SystemBaseTreeNodeModel parent, 
            WorkingTreeModel owner,
            SystemBaseType type,
            INotificationService notificationService,
            IPropertiesPolicy<TreeLeaveModel> propertiesPolicy) 
            : base(uuid, parent, owner, notificationService, propertiesPolicy)
        {
            SystemBaseType = type;
        }
    }
}
