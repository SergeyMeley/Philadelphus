using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    public class WorkingTreeExportDTO
    {
        public string Name { get; }
        public TreeRootExportDTO ContentRoot { get; }
        public List<TreeNodeExportDTO> AllNodes { get; }
        public List<TreeLeaveExportDTO> AllLeaves { get; }

        public WorkingTreeExportDTO() 
        { 
        }
        public WorkingTreeExportDTO(WorkingTreeModel tree)
        {
            Name = tree.Name;
            ContentRoot = new TreeRootExportDTO(tree.ContentRoot);

            AllNodes = tree.GetAllNodesRecursive()
                .Select(node => new TreeNodeExportDTO(node))
                .ToList();

            AllLeaves = tree.GetAllLeavesRecursive()
                .Select(leave => new TreeLeaveExportDTO(leave))
                .ToList();
        }
    }
}
