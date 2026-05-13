using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    public class TreeLeaveExportDTO
    {
        public string Name { get; }
        public string Description { get; }
        public string OwningNodeName { get; }
        public List<AttributeExportDTO> Attributes { get; } = new();

        public TreeLeaveExportDTO(TreeLeaveModel leave)
        {
            ArgumentNullException.ThrowIfNull(leave);

            Name = leave.Name;
            Description = leave.Description;
            OwningNodeName = leave.ParentNode?.Name ?? "Неизвестный";
            Attributes = leave.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
        }

        public TreeLeaveExportDTO(string name, string description, string owningNodeName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentException.ThrowIfNullOrWhiteSpace(owningNodeName);

            Name = name;
            Description = description;
            OwningNodeName = owningNodeName;
        }
    }
}
