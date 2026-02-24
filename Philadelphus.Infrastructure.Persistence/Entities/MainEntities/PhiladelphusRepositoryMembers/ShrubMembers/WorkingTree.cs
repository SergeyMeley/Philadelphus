using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
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

        /// <summary>
        /// Рабочее дерево
        /// </summary>
        public WorkingTree()
        {
            
        }
    }
}
