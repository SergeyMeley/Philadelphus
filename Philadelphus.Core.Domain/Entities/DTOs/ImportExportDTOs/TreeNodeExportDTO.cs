using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

/// <summary>
/// DTO для передачи данных узла рабочего дерева.
/// </summary>
public class TreeNodeExportDTO
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Наименование владеющего корня.
    /// </summary>
    public string OwningRootName { get; set; } = string.Empty;

    /// <summary>
    /// Выполняет операцию ChildLeaves.
    /// </summary>
    /// <returns>Коллекция полученных данных.</returns>
    public List<TreeLeaveExportDTO> ChildLeaves { get; set; } = new();

    /// <summary>
    /// Выполняет операцию Attributes.
    /// </summary>
    /// <returns>Коллекция полученных данных.</returns>
    public List<AttributeExportDTO> Attributes { get; set; } = new();

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TreeNodeExportDTO" />.
    /// </summary>
    public TreeNodeExportDTO() { }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TreeNodeExportDTO" />.
    /// </summary>
    /// <param name="node">Узел рабочего дерева.</param>
    /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
    public TreeNodeExportDTO(TreeNodeModel node)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(node.ChildLeaves);

        Name = node.Name;
        Description = node.Description;
        OwningRootName = node.OwningWorkingTree?.ContentRoot?.Name ?? "Неизвестный";
        ChildLeaves = node.ChildLeaves.Select(l => new TreeLeaveExportDTO(l)).ToList();
        Attributes = node.Attributes?.Select(a => new AttributeExportDTO(a)).ToList() ?? new();
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TreeNodeExportDTO" />.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="description">Описание.</param>
    /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
    /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
    public TreeNodeExportDTO(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);

        Name = name;
        Description = description;
    }
}
