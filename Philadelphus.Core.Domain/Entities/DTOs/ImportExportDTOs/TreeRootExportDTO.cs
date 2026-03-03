using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    public class TreeRootExportDTO
    {
        public string Name { get; }
        public string Description { get; }
        public List<TreeNodeExportDTO> ChildNodes { get; } = new();
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        public TreeRootExportDTO(TreeRootModel root)
        {
            Name = root.Name;
            Description = root.Description;
            ChildNodes = root.ChildNodes?.Select(n => new TreeNodeExportDTO(n)).ToList() ?? new();
            Attributes = root.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
        }
        public TreeRootExportDTO(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
