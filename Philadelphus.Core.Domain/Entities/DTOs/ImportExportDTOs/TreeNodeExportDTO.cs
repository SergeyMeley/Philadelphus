using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

public class TreeNodeExportDTO
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwningRootName { get; set; } = string.Empty;
    public List<TreeLeaveExportDTO> ChildLeaves { get; set; } = new();
    public List<AttributeExportDTO> Attributes { get; set; } = new();

    public TreeNodeExportDTO() { }
    public TreeNodeExportDTO(TreeNodeModel node)
    {
        Name = node.Name;
        Description = node.Description;
        OwningRootName = node.OwningWorkingTree?.ContentRoot?.Name ?? "Неизвестный";
        ChildLeaves = node.ChildLeaves.Select(l => new TreeLeaveExportDTO(l)).ToList();
        Attributes = node.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
    }
}
