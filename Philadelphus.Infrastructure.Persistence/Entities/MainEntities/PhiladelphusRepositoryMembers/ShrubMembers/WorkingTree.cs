using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    /// <summary>
    /// Представляет объект рабочего дерева.
    /// </summary>
    public class WorkingTree : MainEntityBase
    {
        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        public Guid OwnDataStorageUuid { get; set; }

        /// <summary>
        /// Содержимый содержимого.
        /// </summary>
        public virtual TreeRoot ContentRoot { get; set; }
        
        /// <summary>
        /// Содержимые узлы.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public virtual ICollection<TreeNode> ContentNodes { get; set; } = new List<TreeNode>();
        
        /// <summary>
        /// Содержимые листы.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public virtual ICollection<TreeLeave> ContentLeaves { get; set; } = new List<TreeLeave>();
        
        /// <summary>
        /// Содержимые атрибуты.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public virtual ICollection<ElementAttribute> ContentAttributes { get; set; } = new List<ElementAttribute>();

        /// <summary>
        /// Рабочее дерево
        /// </summary>
        public WorkingTree()
        {
            
        }
    }
}
