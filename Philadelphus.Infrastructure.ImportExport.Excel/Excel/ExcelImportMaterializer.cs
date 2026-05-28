using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Infrastructure.ImportExport.Excel
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
            // Фиксированное правило импорта: вся книга Excel материализуется как единственный корень чубушника.
            var root = new DomainRootPayload
            {
                Name = schema.RootName,
                Description = $"Импортировано из книги «{System.IO.Path.GetFileNameWithoutExtension(schema.SourceFilePath)}»"
            };

            root.Attributes.AddRange(_propertyResolver.ResolveRootAttributes(graph.EntitySets));

            var nodeBySourceName = graph.EntitySets
                .ToDictionary(
                    set => set.Definition.SourceName,
                    set => BuildTableNode(set, graph, root.Name),
                    StringComparer.OrdinalIgnoreCase);

            // Связи наследования Excel-таблиц переносятся в иерархию узлов чубушника.
            foreach (var relation in graph.Relations)
            {
                if (nodeBySourceName.TryGetValue(relation.ParentSourceName, out var parentNode) == false
                    || nodeBySourceName.TryGetValue(relation.ChildSourceName, out var childNode) == false)
                {
                    continue;
                }

                if (parentNode.ChildNodes.Any(x => ReferenceEquals(x, childNode)) == false)
                {
                    parentNode.ChildNodes.Add(childNode);
                }
            }

            var childSourceNames = graph.Relations
                .Select(x => x.ChildSourceName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Таблицы без родителя становятся узлами первого уровня под корнем книги.
            foreach (var entitySet in graph.EntitySets)
            {
                if (childSourceNames.Contains(entitySet.Definition.SourceName))
                    continue;

                root.ChildNodes.Add(nodeBySourceName[entitySet.Definition.SourceName]);
            }

            return new DomainWorkingTreePayload
            {
                Name = schema.RootName,
                ContentRoot = root
            };
        }

        private DomainNodePayload BuildTableNode(ImportedEntitySet entitySet, ImportGraph graph, string owningRootName)
        {
            var definition = entitySet.Definition;
            var nodeName = string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.SourceName
                : definition.DisplayName;

            var node = new DomainNodePayload
            {
                Name = nodeName,
                Description = $"Импортировано из таблицы Excel «{definition.SourceName}»",
                OwningRootName = owningRootName
            };

            var fkProperties = GetForeignKeyProperties(definition, graph).ToList();
            var fkPropertyIndexes = fkProperties
                .Select(x => x.Property.ColumnIndex)
                .ToHashSet();

            // FK-колонки не добавляются как обычные атрибуты листа: они будут ссылками на листы родительского узла.
            AddAttributesUnique(
                node.Attributes,
                _propertyResolver.ResolveAttributeDefinitions(
                    entitySet.Rows,
                    ExcelImportPropertyPlacement.Leaf,
                    fkPropertyIndexes));

            // Описание FK-атрибута хранится на дочернем узле, а ссылочный тип указывает на родительский узел.
            foreach (var fkProperty in fkProperties)
            {
                AddAttributesUnique(
                    node.Attributes,
                    new[]
                    {
                        new ExcelImportAttributePayload
                        {
                            Name = fkProperty.Property.PropertyName,
                            Description = $"Ссылка на строку таблицы «{fkProperty.ParentNodeName}»",
                            DataTypeNodeName = fkProperty.ParentNodeName,
                            ValueLeaveName = null!
                        }
                    });
            }

            foreach (var row in entitySet.Rows)
            {
                node.ChildLeaves.Add(BuildLeaf(row, node.Name, fkProperties, fkPropertyIndexes));
            }

            return node;
        }

        private DomainLeafPayload BuildLeaf(
            ImportedEntityRow row,
            string owningNodeName,
            IReadOnlyCollection<ForeignKeyProperty> fkProperties,
            IReadOnlySet<int> fkPropertyIndexes)
        {
            var leaf = new DomainLeafPayload
            {
                Name = ResolveName(row),
                Description = ResolveDescription(row),
                OwningNodeName = owningNodeName
            };

            foreach (var attribute in _propertyResolver.ResolveAttributes(
                         row,
                         ExcelImportPropertyPlacement.Leaf,
                         parentRow: null,
                         excludedColumnIndexes: fkPropertyIndexes))
            {
                leaf.Attributes.Add(attribute);
            }

            // Для каждой строки дочерней таблицы FK-значение заменяется ссылкой на найденный лист родительской таблицы.
            foreach (var fkProperty in fkProperties)
            {
                if (row.ValuesByColumnIndex.TryGetValue(fkProperty.Property.ColumnIndex, out var childKeyValue) == false
                    || string.IsNullOrWhiteSpace(childKeyValue))
                {
                    continue;
                }

                if (fkProperty.ParentLeafNameByKey.TryGetValue(childKeyValue, out var parentLeafName) == false)
                    continue;

                leaf.Attributes.Add(new ExcelImportAttributePayload
                {
                    Name = fkProperty.Property.PropertyName,
                    Description = $"Ссылка на строку таблицы «{fkProperty.ParentNodeName}»",
                    DataTypeNodeName = fkProperty.ParentNodeName,
                    ValueLeaveName = parentLeafName
                });
            }

            return leaf;
        }

        private IEnumerable<ForeignKeyProperty> GetForeignKeyProperties(ImportedEntityDefinition definition, ImportGraph graph)
        {
            foreach (var relation in graph.Relations.Where(x =>
                         string.Equals(x.ChildSourceName, definition.SourceName, StringComparison.OrdinalIgnoreCase)))
            {
                var childProperty = FindProperty(definition, relation.ChildKeyColumnName);
                if (childProperty == null)
                    continue;

                var parentSet = graph.EntitySets.FirstOrDefault(x =>
                    string.Equals(x.Definition.SourceName, relation.ParentSourceName, StringComparison.OrdinalIgnoreCase));
                if (parentSet == null)
                    continue;

                var parentKeyProperty = FindProperty(parentSet.Definition, relation.ParentKeyColumnName);
                if (parentKeyProperty == null)
                    continue;

                var parentNodeName = string.IsNullOrWhiteSpace(parentSet.Definition.DisplayName)
                    ? parentSet.Definition.SourceName
                    : parentSet.Definition.DisplayName;

                // Индекс нужен для быстрого поиска родительского листа по значению внешнего ключа из дочерней строки.
                var parentLeafNameByKey = parentSet.Rows
                    .Select(row => new
                    {
                        Key = GetValue(row, parentKeyProperty),
                        LeafName = ResolveName(row)
                    })
                    .Where(x => string.IsNullOrWhiteSpace(x.Key) == false)
                    .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First().LeafName,
                        StringComparer.OrdinalIgnoreCase);

                yield return new ForeignKeyProperty(childProperty, parentNodeName, parentLeafNameByKey);
            }
        }

        private static PropertyDefinition? FindProperty(ImportedEntityDefinition definition, string columnName)
        {
            return definition.Properties.FirstOrDefault(x =>
                string.Equals(x.SourceColumnName, columnName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.PropertyName, columnName, StringComparison.OrdinalIgnoreCase));
        }

        private static void AddAttributesUnique(
            List<ExcelImportAttributePayload> target,
            IEnumerable<ExcelImportAttributePayload> source)
        {
            foreach (var attribute in source)
            {
                if (target.Any(x => string.Equals(x.Name, attribute.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                target.Add(attribute);
            }
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

        private static string GetValue(ImportedEntityRow row, PropertyDefinition property)
        {
            return row.ValuesByColumnIndex.TryGetValue(property.ColumnIndex, out var value)
                ? value
                : string.Empty;
        }

        private sealed record ForeignKeyProperty(
            PropertyDefinition Property,
            string ParentNodeName,
            IReadOnlyDictionary<string, string> ParentLeafNameByKey);
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

        public List<ExcelImportAttributePayload> Attributes { get; set; } = new();

        public List<DomainNodePayload> ChildNodes { get; set; } = new();
    }

    internal sealed class DomainNodePayload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string OwningRootName { get; set; } = string.Empty;

        public List<ExcelImportAttributePayload> Attributes { get; set; } = new();

        public List<DomainNodePayload> ChildNodes { get; set; } = new();

        public List<DomainLeafPayload> ChildLeaves { get; set; } = new();
    }

    internal sealed class DomainLeafPayload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string OwningNodeName { get; set; } = string.Empty;

        public List<ExcelImportAttributePayload> Attributes { get; set; } = new();
    }

    internal sealed class ExcelImportAttributePayload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DataTypeNodeName { get; set; } = "Не определён";

        public string? ValueLeaveName { get; set; } = "Не задано";
    }
}
