using AutoMapper.Internal;
using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Полностью сгенерировано нейронкой, корректность не гарантируется
    /// </summary>
    public static class JsonImportExportHelper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,       // A-Z, 0-9, знаки препинания
                UnicodeRanges.Cyrillic),        // А-Я, а-я
            Converters = { new JsonStringEnumConverter() },
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string GetJson(WorkingTreeModel tree)
        {
            var exportDto = new WorkingTreeExportDTO(tree);
            return JsonSerializer.Serialize(exportDto, _options);
        }

        public static WorkingTreeModel ParseJson(string json, IPhiladelphusRepositoryService service, PhiladelphusRepositoryModel repository)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // ✅ БЕЗОПАСНО читаем name дерева
            var treeName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString() ?? "Импортированное дерево"
                : "Импортированное дерево";

            // ✅ Читаем корень
            if (!root.TryGetProperty("contentRoot", out var contentRootElement))
                throw new InvalidOperationException("Нет contentRoot");

            var rootName = contentRootElement.TryGetProperty("name", out var rootNameProp)
                ? rootNameProp.GetString() ?? "Корень"
                : "Корень";

            // 1. Создаём дерево
            var dataStorage = repository.DataStorages.First();
            var treeRoot = service.CreateTreeRoot(repository, dataStorage);
            treeRoot.Name = rootName;

            var attributeLinkMap = new Dictionary<Guid, (string, string)>();

            // 2. Создаём узлы корня
            if (contentRootElement.TryGetProperty("childNodes", out var childNodesElement) &&
                childNodesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var nodeElement in childNodesElement.EnumerateArray())
                {
                    if (nodeElement.TryGetProperty("name", out var nodeNameProp))
                    {
                        var node = service.CreateTreeNode(treeRoot);
                        node.Name = nodeNameProp.GetString() ?? "Узел";

                        // Листы узла
                        CreateLeavesFromNode(service, node, nodeElement, attributeLinkMap);

                        // Атрибуты узла
                        CreateAttributesFromElement(service, node, nodeElement, attributeLinkMap);
                    }
                }
            }

            // 3. Атрибуты корня
            CreateAttributesFromElement(service, treeRoot, contentRootElement, attributeLinkMap);

            return treeRoot.OwningWorkingTree;
        }

        private static void CreateLeavesFromNode(IPhiladelphusRepositoryService service, TreeNodeModel node, JsonElement nodeElement, Dictionary<Guid, (string, string)> attributeLinkMap)
        {
            if (nodeElement.TryGetProperty("childLeaves", out var leavesElement) &&
                leavesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var leafElement in leavesElement.EnumerateArray())
                {
                    if (leafElement.TryGetProperty("name", out var leafNameProp))
                    {
                        var leaf = service.CreateTreeLeave(node);
                        leaf.Name = leafNameProp.GetString() ?? "Лист";
                    }
                }
            }
        }

        private static void CreateAttributesFromElement(IPhiladelphusRepositoryService service, IAttributeOwnerModel element, JsonElement elementJson, Dictionary<Guid, (string, string)> attributeLinkMap)
        {
            if (elementJson.TryGetProperty("attributes", out var attributesElement) &&
                attributesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var attrElement in attributesElement.EnumerateArray())
                {
                    var attr = service.CreateElementAttribute(element);

                    attr.Name = attrElement.TryGetProperty("name", out var nameProp)
                        ? nameProp.GetString() ?? "Атрибут"
                        : "Атрибут";

                    attr.IsCollectionValue = attrElement.TryGetProperty("isCollectionValue", out var isCollProp)
                        && isCollProp.GetBoolean();

                    // Сохраняем ссылки
                    var dataTypeName = attrElement.TryGetProperty("dataTypeNodeName", out var dtProp)
                        ? dtProp.GetString() ?? "Не определён"
                        : "Не определён";

                    var valueLeafName = attrElement.TryGetProperty("valueLeaveName", out var vlProp)
                        ? vlProp.GetString() ?? "Не задано"
                        : "Не задано";

                    attributeLinkMap[attr.Uuid] = (dataTypeName, valueLeafName);
                }
            }
        }
    }
}