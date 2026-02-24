using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public class SystemBaseTreeLeaveModel : TreeLeaveModel
    {
        private string _stringValue;

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
            SystemBaseType type) 
            : base(uuid, parent, owner, new TreeLeave())
        {
            SystemBaseType = type;
        }
    }
}
