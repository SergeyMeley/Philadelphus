using Npgsql.Internal.Postgres;
using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Xml.Linq;

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

            if (!root.TryGetProperty("contentRoot", out var contentRootElement))
                throw new InvalidOperationException("Нет contentRoot");

            // 1. Создаём дерево
            var dataStorage = repository.DataStorages.Where(x => x.HasShrubMembersInfrastructureRepository).First();

            var treeName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString() ?? "Импортированное дерево"
                : "Импортированное дерево";
            var tree = service.CreateWorkingTree(repository, dataStorage, needAutoName: false, withoutInfoNotifications: true);
            tree.Name = treeName;

            var rootName = contentRootElement.TryGetProperty("name", out var rootNameProp) ? rootNameProp.GetString() : string.Empty;
            var needRootName = string.IsNullOrEmpty(rootName);
            var treeRoot = service.CreateTreeRoot(tree, needAutoName: needRootName, withoutInfoNotifications: true);
            if (needRootName == false)
            {
                treeRoot.Name = rootName;
            }

            var attributeLinkMap = new Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)>();

            // 2. Создаём структуру + сохраняем атрибуты для привязки
            if (contentRootElement.TryGetProperty("childNodes", out var childNodesElement) &&
                childNodesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var nodeElement in childNodesElement.EnumerateArray())
                {
                    CreateNodeRecursive(service, treeRoot, nodeElement, attributeLinkMap);
                }
            }

            // 3. Атрибуты корня
            CreateAttributesFromElement(service, treeRoot, contentRootElement, attributeLinkMap);

            // ✅ 4. Загружаем полную структуру (узлы + листы)
            //service.GetWorkingTreeContent(treeRoot.OwningWorkingTree);
            //treeRoot.OwningWorkingTree.ContentRoot = treeRoot;

            // ✅ 5. ПРИВЯЗЫВАЕМ типы данных и значения к атрибутам!
            LinkAttributesToRealEntities(service, treeRoot, attributeLinkMap);

            // 6. Загружаем атрибуты
            //service.DistributeAttributes(treeRoot);

            return treeRoot.OwningWorkingTree;
        }

        private static void CreateNodeRecursive(IPhiladelphusRepositoryService service, IParentModel parent, JsonElement nodeElement, Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap)
        {
            var name = nodeElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : string.Empty;
            var needName = string.IsNullOrEmpty(name);

            var node = service.CreateTreeNode(parent, needAutoName: needName, withoutInfoNotifications: true);
            if (needName == false)
            {
                node.Name = name;
            }

            CreateAttributesFromElement(service, node, nodeElement, attributeLinkMap);

            if (nodeElement.TryGetProperty("childNodes", out var childNodes) && childNodes.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in childNodes.EnumerateArray())
                    CreateNodeRecursive(service, node, child, attributeLinkMap);
            }

            CreateLeavesFromNode(service, node, nodeElement, attributeLinkMap);
        }

        private static void CreateLeavesFromNode(IPhiladelphusRepositoryService service, TreeNodeModel node, JsonElement nodeElement, Dictionary<Guid, (string, string, ElementAttributeModel)> attributeLinkMap)
        {
            if (!nodeElement.TryGetProperty("childLeaves", out var leavesElement) || leavesElement.ValueKind != JsonValueKind.Array)
                return;

            foreach (var leafElement in leavesElement.EnumerateArray())
            {
                var name = leafElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : string.Empty;
                var needName = string.IsNullOrEmpty(name);

                var leaf = service.CreateTreeLeave(node, needAutoName: needName, withoutInfoNotifications: true);
                if (needName == false)
                {
                    leaf.Name = name;
                }

                CreateAttributesFromElement(service, leaf, leafElement, attributeLinkMap);
            }
        }

        private static void CreateAttributesFromElement(IPhiladelphusRepositoryService service, IAttributeOwnerModel element, JsonElement elementJson, Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap)
        {
            if (!elementJson.TryGetProperty("attributes", out var attributesElement) || attributesElement.ValueKind != JsonValueKind.Array)
                return;

            foreach (var attrElement in attributesElement.EnumerateArray())
            {
                // Заполняем свойства
                var name = attrElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : string.Empty;
                var needName = string.IsNullOrEmpty(name);

                var attr = element.Attributes.ToList().FirstOrDefault(x => x.Name == name);
                if (attr == null)
                {
                    attr = service.CreateElementAttribute(element, needAutoName: needName, withoutInfoNotifications: true);
                    if (needName == false)
                    {
                        attr.Name = name;
                    }
                    if (element is TreeRootModel)
                    {
                        attr.Override = OverrideType.Abstract;
                    }
                    else if (element is TreeNodeModel)
                    {
                        attr.Override = OverrideType.Virtual;
                    }
                    else if (element is TreeLeaveModel)
                    {
                        attr.Override = OverrideType.NotApplicable;
                    }
                }

                if (attrElement.TryGetProperty("isCollectionValue", out var isCollProp)) attr.IsCollectionValue = isCollProp.GetBoolean();

                if (attrElement.TryGetProperty("visibility", out var visibilityProp))
                {
                    var visibilityString = visibilityProp.GetString();
                    Enum.TryParse<VisibilityScope>(visibilityString, true, out var visibility);
                    attr.Visibility = visibility;
                }

                if (attrElement.TryGetProperty("override", out var overrideProp))
                {
                    var overrideString = overrideProp.GetString();
                    Enum.TryParse<OverrideType>(overrideString, true, out var overrideValue);
                    attr.Override = overrideValue;
                }

                if (attrElement.TryGetProperty("description", out var descProp))
                    attr.Description = descProp.GetString();

                // ✅ СОХРАНЯЕМ АТРИБУТ + ССЫЛКИ для привязки
                var dataTypeName = attrElement.TryGetProperty("dataTypeNodeName", out var dtProp) ? dtProp.GetString() ?? "Текст" : "Текст";
                dataTypeName = dataTypeName == "Строка" ? "Текст" : dataTypeName;
                var valueLeafName = attrElement.TryGetProperty("valueLeaveName", out var vlProp)
                    && vlProp.ValueKind != JsonValueKind.Null
                    ? vlProp.ToString()  // Работает для всех типов!
                    : null;

                attributeLinkMap[attr.LocalUuid] = (dataTypeName, valueLeafName, attr);
           }
        }

        /// <summary>
        /// ✅ ГЛАВНЫЙ МЕТОД: Привязывает реальные DataType и ValueLeaf к атрибутам
        /// </summary>
        private static void LinkAttributesToRealEntities(IPhiladelphusRepositoryService service, TreeRootModel treeRoot, Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap)
        {
            // Получаем ВСЕ узлы и листы дерева
            var allNodes = treeRoot.OwningShrub.ContentWorkingTrees.SelectMany(x => x.ContentRoot.GetAllNodesRecursive())?.ToList();
            var allLeaves = treeRoot.OwningShrub.ContentWorkingTrees.SelectMany(x => x.ContentRoot.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>())?.ToList();

            foreach (var kvp in attributeLinkMap)
            {
                var attr = kvp.Value.attribute;
                var (dataTypeName, valueLeafName, attribute) = kvp.Value;

                if (attr.Owner is IShrubMemberModel sm)
                {
                    var ownAtt = sm.Attributes.SingleOrDefault(x => x.Name == attr.Name) ?? throw new Exception();

                    ownAtt.ValueType = allNodes.SingleOrDefault(x => x.Name == dataTypeName);
                    ownAtt.Value = allLeaves.Where(x => x.ParentNode.Uuid == ownAtt.ValueType.Uuid).FirstOrDefault(x => x.Name == valueLeafName);

                    if (string.IsNullOrEmpty(valueLeafName) == false)
                    {
                        if (ownAtt.Value == null)
                        {
                            var newValue = service.CreateTreeLeave(ownAtt.ValueType, needAutoName: false, withoutInfoNotifications: true);
                            newValue.Name = valueLeafName;
                            ownAtt.Value = newValue;
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
                
            }
        }
    }
}
