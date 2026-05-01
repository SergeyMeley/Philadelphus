using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Linq;
using System.Text.Json;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportPreviewService
    {
        public ImportTreePreviewModel BuildPreview(string json)
        {
            using var document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;

            if (rootElement.TryGetProperty("contentRoot", out var contentRootElement) == false)
                throw new InvalidOperationException("Не удалось найти contentRoot в результате конвертации.");

            var rootItem = BuildRoot(contentRootElement);
            return new ImportTreePreviewModel
            {
                Root = rootItem,
                NodeCount = rootItem.Childs.Count,
                LeafCount = rootItem.Childs.Sum(GetLeafCount)
            };
        }

        private static int GetLeafCount(ImportTreePreviewItem item)
        {
            if (item.ItemType == ImportTreePreviewItemType.Leaf)
                return 1;

            return item.Childs.Sum(GetLeafCount);
        }

        private static ImportTreePreviewItem BuildRoot(JsonElement rootElement)
        {
            var result = new ImportTreePreviewItem
            {
                ItemType = ImportTreePreviewItemType.Root,
                DisplayType = "Корень",
                Name = GetString(rootElement, "name"),
                Description = GetString(rootElement, "description"),
                Attributes = BuildAttributes(rootElement)
            };

            if (rootElement.TryGetProperty("childNodes", out var nodesElement) && nodesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var nodeElement in nodesElement.EnumerateArray())
                {
                    result.Childs.Add(BuildNode(nodeElement));
                }
            }

            return result;
        }

        private static ImportTreePreviewItem BuildNode(JsonElement nodeElement)
        {
            var result = new ImportTreePreviewItem
            {
                ItemType = ImportTreePreviewItemType.Node,
                DisplayType = "Узел",
                Name = GetString(nodeElement, "name"),
                Description = GetString(nodeElement, "description"),
                Attributes = BuildAttributes(nodeElement)
            };

            if (nodeElement.TryGetProperty("childLeaves", out var leavesElement) && leavesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var leafElement in leavesElement.EnumerateArray())
                {
                    result.Childs.Add(BuildLeaf(leafElement));
                }
            }

            return result;
        }

        private static ImportTreePreviewItem BuildLeaf(JsonElement leafElement)
        {
            var result = new ImportTreePreviewItem
            {
                ItemType = ImportTreePreviewItemType.Leaf,
                DisplayType = "Лист",
                Name = GetString(leafElement, "name"),
                Description = GetString(leafElement, "description"),
                Attributes = BuildAttributes(leafElement)
            };

            if (leafElement.TryGetProperty("sequence", out var sequenceElement) && sequenceElement.TryGetInt64(out var sequence))
            {
                result.Sequence = sequence;
            }

            return result;
        }

        private static System.Collections.Generic.List<ImportTreePreviewAttribute> BuildAttributes(JsonElement ownerElement)
        {
            var result = new System.Collections.Generic.List<ImportTreePreviewAttribute>();
            if (ownerElement.TryGetProperty("attributes", out var attributesElement) == false || attributesElement.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var attributeElement in attributesElement.EnumerateArray())
            {
                var visibility = VisibilityScope.Public;
                if (attributeElement.TryGetProperty("visibility", out var visibilityElement))
                {
                    Enum.TryParse(visibilityElement.GetString(), true, out visibility);
                }

                var overrideType = OverrideType.None;
                if (attributeElement.TryGetProperty("override", out var overrideElement))
                {
                    Enum.TryParse(overrideElement.GetString(), true, out overrideType);
                }

                result.Add(new ImportTreePreviewAttribute
                {
                    Name = GetString(attributeElement, "name"),
                    Description = GetString(attributeElement, "description"),
                    DataTypeNodeName = GetString(attributeElement, "dataTypeNodeName"),
                    ValueLeaveName = GetNullableString(attributeElement, "valueLeaveName"),
                    IsCollectionValue = attributeElement.TryGetProperty("isCollectionValue", out var collectionElement) && collectionElement.GetBoolean(),
                    Visibility = visibility,
                    Override = overrideType
                });
            }

            return result;
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var propertyElement) ? propertyElement.GetString() ?? string.Empty : string.Empty;
        }

        private static string? GetNullableString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var propertyElement) == false || propertyElement.ValueKind == JsonValueKind.Null)
                return null;

            return propertyElement.ToString();
        }
    }
}
