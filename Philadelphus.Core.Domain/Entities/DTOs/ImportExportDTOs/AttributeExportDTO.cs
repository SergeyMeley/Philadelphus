using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    /// <summary>
    /// DTO для передачи данных AttributeExportDTO.
    /// </summary>
    public class AttributeExportDTO
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; }

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
        public bool IsCollectionValue { get; } = false;

        /// <summary>
        /// Область видимости.
        /// </summary>
        public VisibilityScope Visibility { get; } = VisibilityScope.Public;

        /// <summary>
        /// Режим переопределения.
        /// </summary>
        public OverrideType Override { get; } = OverrideType.Sealed;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AttributeExportDTO" />.
        /// </summary>
        /// <param name="attr"></param>
        public AttributeExportDTO(ElementAttributeModel attr)
        {
            ArgumentNullException.ThrowIfNull(attr);

            Name = attr.Name;
            Description = attr.Description;
            DataTypeNodeName = attr.ValueType?.Name ?? "Не определён";
            ValueLeaveName = attr.Value?.Name ?? "Не задано";
            IsCollectionValue = attr.IsCollectionValue;
            Visibility = attr.Visibility;
            Override = attr.Override;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AttributeExportDTO" />.
        /// </summary>
        public AttributeExportDTO(string name, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(description);

            Name = name;
            Description = description;
        }
    }
}
