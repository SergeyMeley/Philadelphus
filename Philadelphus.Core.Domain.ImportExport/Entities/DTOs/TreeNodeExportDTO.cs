namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
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
        /// Дочерние узлы.
        /// </summary>
        public List<TreeNodeExportDTO> ChildNodes { get; set; } = new();

        /// <summary>
        /// Дочерние листья.
        /// </summary>
        public List<TreeLeaveExportDTO> ChildLeaves { get; set; } = new();

        /// <summary>
        /// Атрибуты узла.
        /// </summary>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeNodeExportDTO" />.
        /// </summary>
        public TreeNodeExportDTO()
        {
        }
    }
}
