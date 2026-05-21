using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    internal sealed class ExcelImportPropertyResolver
    {
        public IEnumerable<AttributeExportDTO> ResolveAttributes(
            ImportedEntityRow row,
            ExcelImportPropertyPlacement placement,
            ImportedEntityRow? parentRow)
        {
            foreach (var property in row.Definition.Properties.Where(x =>
                         x.Role == ExcelImportColumnRole.Attribute
                         && x.Placement == placement))
            {
                var value = ResolveValue(row, property, parentRow);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                yield return CreateAttribute(property, value);
            }
        }

        public IEnumerable<AttributeExportDTO> ResolveRootAttributes(IReadOnlyCollection<ImportedEntitySet> entitySets)
        {
            var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var propertyGroup in entitySets
                         .SelectMany(set => set.Definition.Properties
                             .Where(property => property.Role == ExcelImportColumnRole.Attribute
                                 && property.Placement == ExcelImportPropertyPlacement.Root)
                             .Select(property => new { set.Rows, Property = property }))
                         .GroupBy(x => x.Property.PropertyName, StringComparer.OrdinalIgnoreCase))
            {
                var first = propertyGroup.First().Property;
                if (emitted.Add(first.PropertyName) == false)
                    continue;

                var value = ResolveConstantValue(
                    propertyGroup.SelectMany(x => x.Rows.Select(row => GetValue(row, x.Property))).ToList(),
                    first.DefaultValue);

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                yield return CreateAttribute(first, value);
            }
        }

        public IEnumerable<AttributeExportDTO> ResolveParentConstantAttributes(IReadOnlyCollection<ImportedEntityRow> childRows, ExcelImportPropertyPlacement placement)
        {
            var candidates = childRows
                .SelectMany(row => row.Definition.Properties
                    .Where(property => property.Role == ExcelImportColumnRole.Attribute
                        && property.Placement == placement
                        && property.PropagationMode == ExcelImportValuePropagationMode.ParentConstant)
                    .Select(property => new { Row = row, Property = property }))
                .GroupBy(x => x.Property.PropertyName, StringComparer.OrdinalIgnoreCase);

            foreach (var group in candidates)
            {
                var property = group.First().Property;
                var value = ResolveConstantValue(group.Select(x => GetValue(x.Row, x.Property)).ToList(), property.DefaultValue);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                yield return CreateAttribute(property, value);
            }
        }

        public IEnumerable<AttributeExportDTO> ResolveAttributeDefinitions(IReadOnlyCollection<ImportedEntityRow> rows, ExcelImportPropertyPlacement placement)
        {
            var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in rows
                         .SelectMany(row => row.Definition.Properties)
                         .Where(property => property.Role == ExcelImportColumnRole.Attribute
                             && property.Placement == placement
                             && property.PropagationMode != ExcelImportValuePropagationMode.ParentConstant))
            {
                if (emitted.Add(property.PropertyName) == false)
                    continue;

                yield return CreateAttributeDefinition(property);
            }
        }

        private static string ResolveValue(ImportedEntityRow row, PropertyDefinition property, ImportedEntityRow? parentRow)
        {
            var rawValue = GetValue(row, property);
            return property.PropagationMode switch
            {
                ExcelImportValuePropagationMode.ParentConstant => string.Empty,
                ExcelImportValuePropagationMode.ParentFallbackIfEmpty when string.IsNullOrWhiteSpace(rawValue) && parentRow != null
                    => ResolveParentValue(parentRow, property),
                _ => rawValue
            };
        }

        private static string ResolveParentValue(ImportedEntityRow parentRow, PropertyDefinition childProperty)
        {
            var parentProperty = parentRow.Definition.Properties.FirstOrDefault(x =>
                x.Role == ExcelImportColumnRole.Attribute
                && string.Equals(x.PropertyName, childProperty.PropertyName, StringComparison.OrdinalIgnoreCase));

            if (parentProperty == null)
                return childProperty.DefaultValue?.Trim() ?? string.Empty;

            var parentValue = GetValue(parentRow, parentProperty);
            return string.IsNullOrWhiteSpace(parentValue)
                ? childProperty.DefaultValue?.Trim() ?? string.Empty
                : parentValue;
        }

        private static string ResolveConstantValue(IReadOnlyCollection<string> values, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(defaultValue) == false)
                return defaultValue.Trim();

            var distinctValues = values
                .Where(value => string.IsNullOrWhiteSpace(value) == false)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return distinctValues.Count == 1 ? distinctValues[0] : string.Empty;
        }

        private static string GetValue(ImportedEntityRow row, PropertyDefinition property)
        {
            return row.ValuesByColumnIndex.TryGetValue(property.ColumnIndex, out var value)
                ? value
                : string.Empty;
        }

        private static AttributeExportDTO CreateAttribute(PropertyDefinition property, string value)
        {
            return new AttributeExportDTO(property.PropertyName, ResolveDescription(property))
            {
                DataTypeNodeName = property.DataTypeNodeName,
                ValueLeaveName = value
            };
        }

        private static AttributeExportDTO CreateAttributeDefinition(PropertyDefinition property)
        {
            return new AttributeExportDTO(property.PropertyName, ResolveDescription(property))
            {
                DataTypeNodeName = property.DataTypeNodeName,
                ValueLeaveName = null!
            };
        }

        private static string ResolveDescription(PropertyDefinition property)
        {
            return string.IsNullOrWhiteSpace(property.Description)
                ? $"Импортировано из колонки «{property.SourceColumnName}»"
                : property.Description;
        }
    }
}
