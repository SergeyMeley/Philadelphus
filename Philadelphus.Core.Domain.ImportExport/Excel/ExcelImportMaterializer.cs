using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    internal sealed class ExcelImportMaterializer
    {
        private readonly ExcelImportPropertyResolver _propertyResolver;

        public ExcelImportMaterializer(ExcelImportPropertyResolver propertyResolver)
        {
            _propertyResolver = propertyResolver;
        }

        public DomainWorkingTreePayload Materialize(ExcelImportSchema schema, ImportGraph graph)
        {
            var root = new DomainRootPayload
            {
                Name = schema.RootName,
                Description = $"Импортировано из книги «{System.IO.Path.GetFileNameWithoutExtension(schema.SourceFilePath)}»"
            };

            root.Attributes.AddRange(_propertyResolver.ResolveRootAttributes(graph.EntitySets));

            foreach (var row in GetTopLevelRows(graph))
            {
                MaterializeTopLevelRow(row, graph, root);
            }

            return new DomainWorkingTreePayload
            {
                Name = schema.RootName,
                ContentRoot = root
            };
        }

        private void MaterializeTopLevelRow(ImportedEntityRow row, ImportGraph graph, DomainRootPayload root)
        {
            if (row.Definition.EntityKind == ExcelImportEntityKind.Root)
            {
                root.Attributes.AddRange(_propertyResolver.ResolveAttributes(row, ExcelImportPropertyPlacement.Root, null));
                foreach (var child in GetChildren(row, graph))
                    MaterializeTopLevelRow(child, graph, root);
                return;
            }

            if (row.Definition.EntityKind == ExcelImportEntityKind.Node)
            {
                root.ChildNodes.Add(BuildNode(row, graph, null, root.Name));
                return;
            }

            // В доменной модели leaf не может висеть прямо на root. Это не Excel-контейнер:
            // это явный технический узел текущего root для legacy-сценариев без node-сущности.
            var fallbackNodeName = string.IsNullOrWhiteSpace(row.Definition.DisplayName)
                ? row.Definition.SourceName
                : row.Definition.DisplayName;
            if (string.IsNullOrWhiteSpace(fallbackNodeName))
                fallbackNodeName = root.Name;

            var fallbackNode = root.ChildNodes.FirstOrDefault(x => string.Equals(x.Name, fallbackNodeName, StringComparison.OrdinalIgnoreCase));
            if (fallbackNode == null)
            {
                fallbackNode = new DomainNodePayload
                {
                    Name = fallbackNodeName,
                    Description = "Технический узел для leaf-строк без родительской node-сущности",
                    OwningRootName = root.Name
                };
                root.ChildNodes.Add(fallbackNode);
            }

            AddAttributesUnique(
                fallbackNode.Attributes,
                _propertyResolver.ResolveAttributeDefinitions(new[] { row }, ExcelImportPropertyPlacement.Leaf));
            fallbackNode.ChildLeaves.Add(BuildLeaf(row, null, fallbackNode.Name));
        }

        private DomainNodePayload BuildNode(
            ImportedEntityRow row,
            ImportGraph graph,
            ImportedEntityRow? parentRow,
            string owningRootName)
        {
            var node = new DomainNodePayload
            {
                Name = ResolveName(row),
                Description = ResolveDescription(row),
                OwningRootName = owningRootName
            };

            AddAttributesUnique(node.Attributes, _propertyResolver.ResolveAttributes(row, ExcelImportPropertyPlacement.Node, parentRow));
            var children = GetChildren(row, graph);
            // Leaf-свойства в PH-модели должны иметь определение на родительском node.
            // Иначе значения на leaf не наследуют корректную декларацию атрибута и в UI выглядят пустыми.
            AddAttributesUnique(node.Attributes, _propertyResolver.ResolveAttributeDefinitions(children, ExcelImportPropertyPlacement.Leaf));
            AddAttributesUnique(node.Attributes, _propertyResolver.ResolveParentConstantAttributes(children, ExcelImportPropertyPlacement.Node));

            foreach (var child in children)
            {
                if (child.Definition.EntityKind == ExcelImportEntityKind.Leaf)
                {
                    node.ChildLeaves.Add(BuildLeaf(child, row, node.Name));
                }
                else if (child.Definition.EntityKind == ExcelImportEntityKind.Node)
                {
                    node.ChildNodes.Add(BuildNode(child, graph, row, owningRootName));
                }
                else
                {
                    AddAttributesUnique(node.Attributes, _propertyResolver.ResolveAttributes(child, ExcelImportPropertyPlacement.Node, row));
                }
            }

            return node;
        }

        private static void AddAttributesUnique(
            List<AttributeExportDTO> target,
            IEnumerable<AttributeExportDTO> source)
        {
            foreach (var attribute in source)
            {
                if (target.Any(x => string.Equals(x.Name, attribute.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                target.Add(attribute);
            }
        }

        private TreeLeaveExportDTO BuildLeaf(ImportedEntityRow row, ImportedEntityRow? parentRow, string owningNodeName)
        {
            var leaf = new TreeLeaveExportDTO(ResolveName(row), ResolveDescription(row), owningNodeName);

            foreach (var attribute in _propertyResolver.ResolveAttributes(row, ExcelImportPropertyPlacement.Leaf, parentRow))
            {
                leaf.Attributes.Add(attribute);
            }

            return leaf;
        }

        private static IEnumerable<ImportedEntityRow> GetTopLevelRows(ImportGraph graph)
        {
            return graph.EntitySets
                .SelectMany(x => x.Rows)
                .Where(row => graph.ChildRows.Contains(row) == false)
                // Если сущность объявлена child в relation, но конкретная строка не нашла parent,
                // она не становится самостоятельным root-level элементом. Связь опциональна по данным:
                // совпавшие строки связываются, несовпавшие пропускаются.
                .Where(row => graph.RelatedChildSourceNames.Contains(row.Definition.SourceName) == false);
        }

        private static IReadOnlyList<ImportedEntityRow> GetChildren(ImportedEntityRow row, ImportGraph graph)
        {
            return graph.ChildrenByParent.TryGetValue(row, out var children)
                ? children
                : Array.Empty<ImportedEntityRow>();
        }

        private static string ResolveName(ImportedEntityRow row)
        {
            var nameParts = row.Definition.Properties
                .Where(x => x.Role == ExcelImportColumnRole.SystemName)
                .Select(property => GetValue(row, property))
                .Where(value => string.IsNullOrWhiteSpace(value) == false)
                .ToList();

            if (nameParts.Count > 0)
                return string.Join(" - ", nameParts);

            var firstValue = row.Definition.Properties
                .Where(x => x.Role != ExcelImportColumnRole.Ignore)
                .Select(property => GetValue(row, property))
                .FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);

            return string.IsNullOrWhiteSpace(firstValue)
                ? row.ExcelRowNumber.ToString()
                : firstValue;
        }

        private static string ResolveDescription(ImportedEntityRow row)
        {
            var description = row.Definition.Properties
                .Where(x => x.Role == ExcelImportColumnRole.SystemDescription)
                .Select(property => GetValue(row, property))
                .FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);

            return string.IsNullOrWhiteSpace(description)
                ? $"Импортировано из строки «{row.ExcelRowNumber}»"
                : description;
        }

        private static long? ResolveSequence(ImportedEntityRow row)
        {
            var sequence = row.Definition.Properties
                .Where(x => x.Role == ExcelImportColumnRole.SystemSequence)
                .Select(property => GetValue(row, property))
                .FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);

            return long.TryParse(sequence, out var parsed) ? parsed : null;
        }

        private static string GetValue(ImportedEntityRow row, PropertyDefinition property)
        {
            return row.ValuesByColumnIndex.TryGetValue(property.ColumnIndex, out var value)
                ? value
                : string.Empty;
        }
    }

    internal sealed class DomainWorkingTreePayload
    {
        public string Name { get; set; } = string.Empty;

        public DomainRootPayload ContentRoot { get; set; } = new();
    }

    internal sealed class DomainRootPayload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<AttributeExportDTO> Attributes { get; set; } = new();

        public List<DomainNodePayload> ChildNodes { get; set; } = new();
    }

    internal sealed class DomainNodePayload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string OwningRootName { get; set; } = string.Empty;

        public List<AttributeExportDTO> Attributes { get; set; } = new();

        public List<DomainNodePayload> ChildNodes { get; set; } = new();

        public List<TreeLeaveExportDTO> ChildLeaves { get; set; } = new();
    }
}
