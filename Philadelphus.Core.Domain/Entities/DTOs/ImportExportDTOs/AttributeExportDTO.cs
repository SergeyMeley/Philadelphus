using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs
{
    public class AttributeExportDTO
    {
        public string Name { get; }
        public string Description { get; }
        public string DataTypeNodeName { get; set; } = "Не определён";
        public string ValueLeaveName { get; set; } = "Не задано";
        public bool IsCollectionValue { get; }
        public VisibilityScope Visibility { get; }
        public OverrideType Override { get; }

        public AttributeExportDTO(ElementAttributeModel attr)
        {
            Name = attr.Name;
            Description = attr.Description;
            DataTypeNodeName = attr.ValueType?.Name ?? "Не определён";
            ValueLeaveName = attr.Value?.Name ?? "Не задано";
            IsCollectionValue = attr.IsCollectionValue;
            Visibility = attr.Visibility;
            Override = attr.Override;
        }
    }
}
