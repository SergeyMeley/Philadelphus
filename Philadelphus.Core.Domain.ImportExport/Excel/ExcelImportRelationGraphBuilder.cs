using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    internal sealed class ExcelImportRelationGraphBuilder
    {
        public ImportGraph Build(
            IReadOnlyCollection<ImportedEntitySet> entitySets,
            IReadOnlyCollection<ExcelImportRelationSchema> relations)
        {
            var graph = new ImportGraph
            {
                EntitySets = entitySets.ToList()
            };
            var setsBySource = entitySets.ToDictionary(x => x.Definition.SourceName, StringComparer.OrdinalIgnoreCase);

            foreach (var relation in relations.Where(x => x.IsEnabled))
            {
                graph.RelatedChildSourceNames.Add(relation.ChildSourceName);

                if (setsBySource.TryGetValue(relation.ParentSourceName, out var parentSet) == false
                    || setsBySource.TryGetValue(relation.ChildSourceName, out var childSet) == false)
                {
                    continue;
                }

                if (parentSet.Definition.EntityKind == ExcelImportEntityKind.Leaf)
                    throw new InvalidOperationException($"Сущность «{parentSet.Definition.DisplayName}» является Leaf и не может быть родителем связи.");

                var parentKey = FindProperty(parentSet.Definition, relation.ParentKeyColumnName);
                var childKey = FindProperty(childSet.Definition, relation.ChildKeyColumnName);
                if (parentKey == null || childKey == null)
                    continue;

                var parentsByKey = parentSet.Rows
                    .Select(row => new { Key = GetValue(row, parentKey), Row = row })
                    .Where(x => string.IsNullOrWhiteSpace(x.Key) == false)
                    .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.Select(item => item.Row).ToList(), StringComparer.OrdinalIgnoreCase);

                foreach (var childRow in childSet.Rows)
                {
                    var childKeyValue = GetValue(childRow, childKey);
                    if (string.IsNullOrWhiteSpace(childKeyValue))
                        continue;

                    if (parentsByKey.TryGetValue(childKeyValue, out var parentRows) == false)
                        continue;

                    foreach (var parentRow in parentRows)
                    {
                        if (graph.ChildrenByParent.TryGetValue(parentRow, out var children) == false)
                        {
                            children = new List<ImportedEntityRow>();
                            graph.ChildrenByParent[parentRow] = children;
                        }

                        children.Add(childRow);
                        graph.ChildRows.Add(childRow);
                    }
                }
            }

            return graph;
        }

        private static PropertyDefinition? FindProperty(ImportedEntityDefinition definition, string columnName)
        {
            return definition.Properties.FirstOrDefault(x =>
                string.Equals(x.SourceColumnName, columnName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.PropertyName, columnName, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetValue(ImportedEntityRow row, PropertyDefinition property)
        {
            return row.ValuesByColumnIndex.TryGetValue(property.ColumnIndex, out var value)
                ? value
                : string.Empty;
        }
    }
}
