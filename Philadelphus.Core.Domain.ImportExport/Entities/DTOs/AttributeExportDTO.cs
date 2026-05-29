using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.ImportExport.Entities.DTOs
{
    /// <summary>
    /// DTO для передачи данных атрибута элемента рабочего дерева.
    /// </summary>
    public class AttributeExportDTO
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
        /// Наименование узла типа данных.
        /// </summary>
        public string DataTypeNodeName { get; set; } = "Не определён";

        /// <summary>
        /// Наименование листа значения.
        /// </summary>
        public string ValueLeaveName { get; set; } = "Не задано";

        /// <summary>
        /// Признак коллекции значений.
        /// </summary>
        public bool IsCollectionValue { get; set; } = false;

        /// <summary>
        /// Область видимости.
        /// </summary>
        public VisibilityScope Visibility { get; set; }

        /// <summary>
        /// Режим переопределения.
        /// </summary>
        public OverrideType Override { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AttributeExportDTO" />.
        /// </summary>
        public AttributeExportDTO()
        {
        }
    }
}
