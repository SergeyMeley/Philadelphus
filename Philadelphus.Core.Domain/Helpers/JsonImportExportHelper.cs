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
using System.Text;
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
        /// <summary>
        /// Максимальный размер JSON файла (500 МБ)
        /// </summary>
        private const int MaxJsonSize = 500 * 1024 * 1024;

        /// <summary>
        /// Максимальная глубина JSON структуры
        /// </summary>
        private const int MaxJsonDepth = 100;

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

        private static readonly JsonDocumentOptions _documentOptions = new()
        {
            MaxDepth = MaxJsonDepth,
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow
        };

        public static string GetJson(WorkingTreeModel tree)
        {
            var exportDto = new WorkingTreeExportDTO(tree);
            return JsonSerializer.Serialize(exportDto, _options);
        }

        public static WorkingTreeModel ParseJson(
            string json, 
            IPhiladelphusRepositoryService service,
            PhiladelphusRepositoryModel repository,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(repository);

            // Валидация входных данных
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            var jsonSize = Encoding.UTF8.GetByteCount(json);
            if (jsonSize > MaxJsonSize)
                throw new InvalidOperationException(
                    $"JSON слишком большой: {jsonSize} байт (максимум {MaxJsonSize} байт)");

            refreshProcess?.Invoke("Читаем json");
            refreshProgress?.Invoke(0, 1);

            try
            {
                using var document = JsonDocument.Parse(json, _documentOptions);
                var root = document.RootElement;

                if (!root.TryGetProperty("contentRoot", out var contentRootElement))
                    throw new InvalidOperationException("Нет contentRoot в JSON");

                refreshProgress?.Invoke(1, 1);

                // 1. Создаём дерево
                refreshProcess?.Invoke("Создаём дерево");
                refreshProgress?.Invoke(0, 1);

                var dataStorage = repository.DataStorages?.Where(x => x.HasShrubMembersInfrastructureRepository).FirstOrDefault()
                    ?? throw new InvalidOperationException("Не найдено хранилище с ShrubMembers");

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

                refreshProgress?.Invoke(1, 1);

                // 2. Создаём структуру + сохраняем атрибуты для привязки
                refreshProcess?.Invoke("Создаём структуру");
                refreshProgress?.Invoke(0, 1);

                if (contentRootElement.TryGetProperty("childNodes", out var childNodesElement) &&
                    childNodesElement.ValueKind == JsonValueKind.Array)
                {
                    var nodeElements = childNodesElement.EnumerateArray();
                    var count = nodeElements.Count();
                    int i = 0;

                    foreach (var nodeElement in nodeElements)
                    {
                        CreateNodeRecursive(service, treeRoot, nodeElement, attributeLinkMap);

                        refreshProgress?.Invoke(i++, count);
                    }
                }

                // 3. Атрибуты корня
                refreshProcess?.Invoke("Атрибуты корня");
                refreshProgress?.Invoke(0, 1);

                CreateAttributesFromElement(service, treeRoot, contentRootElement, attributeLinkMap);

                refreshProgress?.Invoke(1, 1);

                // ✅ 4. Загружаем полную структуру (узлы + листы)
                //service.GetWorkingTreeContent(treeRoot.OwningWorkingTree);
                //treeRoot.OwningWorkingTree.ContentRoot = treeRoot;

                // ✅ 5. ПРИВЯЗЫВАЕМ типы данных и значения к атрибутам!
                refreshProcess?.Invoke("Привязываем значения к атрибутам");
                refreshProgress?.Invoke(0, attributeLinkMap.Count());

                LinkAttributesToRealEntities(service, treeRoot, attributeLinkMap, refreshProgress);
            
                refreshProgress?.Invoke(attributeLinkMap.Count(), attributeLinkMap.Count());

                // 6. Загружаем атрибуты
                //service.DistributeAttributes(treeRoot);

                refreshProcess?.Invoke("Готово!");
                refreshProgress?.Invoke(1, 1);

                return treeRoot.OwningWorkingTree;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Ошибка парсинга JSON: {ex.Message}", ex);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(
                    $"Ошибка обработки JSON данных: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Неожиданная ошибка при импорте JSON: {ex.Message}", ex);
            }
        }

        private static void CreateNodeRecursive(
            IPhiladelphusRepositoryService service, 
            IParentModel parent, 
            JsonElement nodeElement,
            Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap)
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

        private static void CreateLeavesFromNode(
            IPhiladelphusRepositoryService service, 
            TreeNodeModel node, 
            JsonElement nodeElement,
            Dictionary<Guid, (string, string, ElementAttributeModel)> attributeLinkMap)
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

        private static void CreateAttributesFromElement(
            IPhiladelphusRepositoryService service, 
            IAttributeOwnerModel element, 
            JsonElement elementJson, 
            Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap)
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
        private static void LinkAttributesToRealEntities(
            IPhiladelphusRepositoryService service,
            TreeRootModel treeRoot,
            Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap,
            Action<int, int> refreshProgress)
        {
            var allNodes = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentRoot.GetAllNodesRecursive())
                .ToList();

            var allLeaves = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentRoot.GetAllLeavesRecursive() ?? new List<TreeLeaveModel>())
                .ToList();

            var nodesByName = allNodes
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.Single());

            var leavesByParentUuidAndName = allLeaves
                .GroupBy(x => (ParentUuid: x.ParentNode.Uuid, LeafName: x.Name))
                .ToDictionary(x => x.Key, x => x.First());

            var count = attributeLinkMap.Count;
            int i = 0;

            var attributesByOwner = new Dictionary<IShrubMemberModel, Dictionary<string, ElementAttributeModel>>();

            foreach (var kvp in attributeLinkMap)
            {
                var (dataTypeName, valueLeafName, attr) = kvp.Value;

                if (attr.Owner is not IShrubMemberModel sm)
                    throw new Exception();

                if (!attributesByOwner.TryGetValue(sm, out var attributesByName))
                {
                    attributesByName = sm.Attributes
                        .GroupBy(x => x.Name)
                        .ToDictionary(x => x.Key, x => x.Single());

                    attributesByOwner[sm] = attributesByName;
                }

                if (!attributesByName.TryGetValue(attr.Name, out var ownAtt))
                    throw new Exception();

                nodesByName.TryGetValue(dataTypeName, out var valueType);

                ownAtt.ValueType = valueType;

                if (valueType != null &&
                    leavesByParentUuidAndName.TryGetValue((valueType.Uuid, valueLeafName), out var value))
                {
                    ownAtt.Value = value;
                }
                else
                {
                    if (!string.IsNullOrEmpty(valueLeafName))
                    {
                        if (valueType == null)
                            throw new InvalidOperationException($"Не найден тип данных '{dataTypeName}' для атрибута '{attr.Name}'");

                        var newValue = service.CreateTreeLeave(
                            valueType,
                            needAutoName: false,
                            withoutInfoNotifications: true);

                        newValue.Name = valueLeafName;
                        ownAtt.Value = newValue;

                        if (valueType != null)
                        {
                            leavesByParentUuidAndName[(valueType.Uuid, valueLeafName)] = newValue;
                        }
                    }
                }

                refreshProgress?.Invoke(i++, count);
            }
        }
    }
}
