namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
    /// <summary>
    /// DTO для передачи данных корня рабочего дерева.
    /// </summary>
    public class TreeRootExportDTO
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
        /// Дочерние узлы.
        /// </summary>
        public List<TreeNodeExportDTO> ChildNodes { get; set; } = new();

        /// <summary>
        /// Атрибуты корня.
        /// </summary>
        public List<AttributeExportDTO> Attributes { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeRootExportDTO" />.
        /// </summary>
        public TreeRootExportDTO()
        {
        }
    }
}
