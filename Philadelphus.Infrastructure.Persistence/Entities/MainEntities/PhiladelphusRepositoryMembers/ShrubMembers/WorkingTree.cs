using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    public class WorkingTree : MainEntityBase
    {
        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        public Guid OwnDataStorageUuid { get; set; }

        public virtual TreeRoot ContentRoot { get; set; }
        public virtual ICollection<TreeNode> ContentNodes { get; set; } = new List<TreeNode>();
        public virtual ICollection<TreeLeave> ContentLeaves { get; set; } = new List<TreeLeave>();
        public virtual ICollection<ElementAttribute> ContentAttributes { get; set; } = new List<ElementAttribute>();

        /// <summary>
        /// Рабочее дерево
        /// </summary>
        public WorkingTree()
        {
            
        }
    }
}
