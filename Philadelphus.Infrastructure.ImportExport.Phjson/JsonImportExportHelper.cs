using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Infrastructure.ImportExport.Phjson
{
    /// <summary>
    /// Полностью сгенерировано нейронкой, корректность не гарантируется
    /// </summary>
    internal static class JsonImportExportHelper
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

        /// <summary>
        /// Сериализует DTO рабочего дерева в JSON.
        /// </summary>
        /// <param name="exportDto">DTO рабочего дерева.</param>
        /// <returns>JSON-представление рабочего дерева.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public static string GetJson(WorkingTreeExportDTO exportDto)
        {
            ArgumentNullException.ThrowIfNull(exportDto);

            return JsonSerializer.Serialize(exportDto, _options);
        }

        /// <summary>
        /// Читает DTO рабочего дерева из JSON.
        /// </summary>
        /// <param name="json">JSON-строка.</param>
        /// <returns>DTO рабочего дерева.</returns>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        /// <exception cref="InvalidOperationException">Если JSON не удалось прочитать.</exception>
        public static WorkingTreeExportDTO ParseDtoFromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);
            ValidateJsonSize(json);

            try
            {
                using var document = JsonDocument.Parse(json, _documentOptions);

                return JsonSerializer.Deserialize<WorkingTreeExportDTO>(json, _options)
                    ?? throw new InvalidOperationException("JSON не содержит данные рабочего дерева.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Ошибка парсинга JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Преобразует .phjson в бизнес-модель.
        /// </summary>
        /// <param name="json">JSON-строка.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="repository">Репозиторий Чубушника.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        /// <exception cref="InvalidOperationException">Если операция недопустима для текущего состояния объекта.</exception>
        public static WorkingTreeModel ParseJson(
            string json, 
            IPhiladelphusRepositoryService service,
            PhiladelphusRepositoryModel repository,
            Action<string> refreshProcess,
            Action<int, int> refreshProgress)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(refreshProcess);
            ArgumentNullException.ThrowIfNull(refreshProgress);

            // Валидация входных данных
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            ValidateJsonSize(json);

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

                // Атрибуты создаются раньше, чем их ссылки на тип данных и значение могут быть разрешены.
                // Поэтому здесь храним "обещание привязки": имя узла-типа, имя листа-значения и сам атрибут.
                // Реальные ValueType и Value будут назначены позже, когда вся структура узлов и листьев уже создана.
                var attributeLinkMap = new Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)>();

                refreshProgress?.Invoke(1, 1);

                // 2. Атрибуты корня должны быть созданы до наследников, чтобы узлы и листья могли их унаследовать.
                // На этом шаге заполняются только собственные свойства атрибута. Ссылочные свойства ValueType и Value
                // намеренно не назначаются сразу: соответствующие узлы и листья могут появиться ниже в JSON.
                refreshProcess?.Invoke("Атрибуты корня");
                refreshProgress?.Invoke(0, 1);

                CreateAttributesFromElement(service, treeRoot, contentRootElement, attributeLinkMap);

                refreshProgress?.Invoke(1, 1);

                // 3. Создаём структуру + сохраняем атрибуты для привязки.
                // Каждый созданный атрибут добавляется в attributeLinkMap, но его ValueType/Value остаются отложенными.
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

                // 4. Загружаем полную структуру (узлы + листы)
                //service.GetWorkingTreeContent(treeRoot.OwningWorkingTree);
                //treeRoot.OwningWorkingTree.ContentRoot = treeRoot;

                // 5. Привязываем типы данных и значения к атрибутам.
                // К этому моменту импорт уже создал все узлы и листья, поэтому поиск по именам может найти
                // как пользовательские типы текущего дерева, так и типы системного дерева.
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

        private static void ValidateJsonSize(string json)
        {
            var jsonSize = Encoding.UTF8.GetByteCount(json);
            if (jsonSize > MaxJsonSize)
                throw new InvalidOperationException(
                    $"JSON слишком большой: {jsonSize} байт (максимум {MaxJsonSize} байт)");
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

                if (leaf is SystemBaseTreeLeaveModel systemBaseLeaf)
                {
                    // В новом формате системные листья хранят значение отдельно от Name. Для старых .phjson,
                    // где stringValue еще не было, сохраняем обратную совместимость и берем значение из name.
                    var stringValue = leafElement.TryGetProperty("stringValue", out var stringValueProp)
                        ? stringValueProp.GetString()
                        : name;

                    systemBaseLeaf.StringValue = stringValue;
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

                // Имена в JSON нормализуются тем же способом, что и при записи Name в доменную модель.
                // Это важно после введения автоматического удаления запрещенных символов:
                // в файле может быть старое имя с символами, которые в модели уже были удалены.
                var normalizedName = NormalizeImportName(name);

                // Сначала пытаемся найти уже существующий атрибут. Для узлов и листьев это обычно
                // унаследованный атрибут, созданный ранее при создании родителя.
                var attr = element.Attributes.ToList().FirstOrDefault(x => NormalizeImportName(x.Name) == normalizedName);
                if (attr == null)
                {
                    if (element is TreeLeaveModel)
                    {
                        // Листья не могут создавать собственные атрибуты. Если атрибут не найден,
                        // значит он не успел или не смог унаследоваться от родителя, и импорт должен упасть явно.
                        throw new InvalidOperationException(
                            $"Не найден унаследованный атрибут '{name}' у листа '{GetElementName(element)}' [{element.Uuid}]. " +
                            "Создание собственных атрибутов для листов запрещено.");
                    }

                    attr = service.CreateElementAttribute(element, needAutoName: needName, withoutInfoNotifications: true);
                    if (attr == null)
                    {
                        throw new InvalidOperationException(
                            $"Не удалось создать атрибут '{name}' для элемента '{GetElementName(element)}' [{element.Uuid}].");
                    }

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
                    if (visibility != VisibilityScope.SystemError)
                    {
                        attr.Visibility = visibility;
                    }
                }

                if (attrElement.TryGetProperty("override", out var overrideProp))
                {
                    var overrideString = overrideProp.GetString();
                    Enum.TryParse<OverrideType>(overrideString, true, out var overrideValue);
                    if (overrideValue != OverrideType.SystemError)
                    {
                        attr.Override = overrideValue;
                    }
                }

                if (attrElement.TryGetProperty("description", out var descProp))
                    attr.Description = descProp.GetString();

                // Сохраняем атрибут и строковые ссылки для поздней привязки.
                // ValueType и Value не задаются здесь, потому что нужные TreeNodeModel/TreeLeaveModel
                // могут быть созданы только после полного обхода JSON-структуры.
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
        /// Привязывает реальные узлы типов данных и листья значений к ранее созданным атрибутам.
        /// </summary>
        /// <remarks>
        /// Импорт разделен на два этапа. Сначала создаются элементы дерева и атрибуты, чтобы механика
        /// наследования могла построить копии атрибутов для дочерних элементов. Затем, когда все узлы и листья
        /// уже существуют, этот метод преобразует строковые имена из JSON в реальные ссылки на доменные модели.
        /// Такой порядок нужен, потому что атрибут корня может ссылаться на тип или значение, объявленные ниже
        /// по JSON-дереву.
        /// </remarks>
        /// <param name="service">Доменный сервис, используемый для создания отсутствующих листьев-значений.</param>
        /// <param name="treeRoot">Корень импортируемого рабочего дерева.</param>
        /// <param name="attributeLinkMap">Отложенные ссылки атрибутов, собранные при создании структуры.</param>
        /// <param name="refreshProgress">Callback обновления прогресса импорта.</param>
        private static void LinkAttributesToRealEntities(
            IPhiladelphusRepositoryService service,
            TreeRootModel treeRoot,
            Dictionary<Guid, (string dataTypeName, string valueLeafName, ElementAttributeModel attribute)> attributeLinkMap,
            Action<int, int> refreshProgress)
        {
            // Ищем типы не только в импортируемом дереве, но и во всех деревьях ShrubModel.
            // Это сохраняет возможность ссылаться на системные типы данных.
            var allNodes = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentNodes ?? Enumerable.Empty<TreeNodeModel>())
                .ToList();

            // Листья также собираются заранее, чтобы быстро искать значение по паре:
            // "родительский тип данных" + "имя листа".
            var allLeaves = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentLeaves ?? Enumerable.Empty<TreeLeaveModel>())
                .ToList();

            var leavesByParentUuidAndName = allLeaves
                .GroupBy(x => (ParentUuid: x.ParentNode.Uuid, LeafName: NormalizeImportName(x.Name)))
                .ToDictionary(x => x.Key, x => x.First());

            var count = attributeLinkMap.Count;
            int i = 0;

            // Кэшируем атрибуты по владельцу. У одного владельца может быть несколько атрибутов,
            // и для каждого из них нужно найти фактический экземпляр, который получит ValueType и Value.
            var attributesByOwner = new Dictionary<IShrubMemberModel, Dictionary<string, ElementAttributeModel>>();

            foreach (var kvp in attributeLinkMap)
            {
                var (dataTypeName, valueLeafName, attr) = kvp.Value;

                if (attr.Owner is not IShrubMemberModel sm)
                    throw new Exception();

                if (!attributesByOwner.TryGetValue(sm, out var attributesByName))
                {
                    attributesByName = BuildAttributesByName(sm);

                    attributesByOwner[sm] = attributesByName;
                }

                // attr может быть как собственным атрибутом, так и унаследованной копией.
                // Поэтому перед записью еще раз находим атрибут в коллекции владельца по нормализованному имени.
                if (!attributesByName.TryGetValue(NormalizeImportName(attr.Name), out var ownAtt))
                    throw new Exception();

                var valueType = ResolveNodeByName(dataTypeName, allNodes, treeRoot.OwningWorkingTree);

                ownAtt.ValueType = valueType;

                // Если лист-значение уже существует под найденным типом данных, просто привязываем его.
                if (valueType != null &&
                    leavesByParentUuidAndName.TryGetValue((valueType.Uuid, NormalizeImportName(valueLeafName)), out var value))
                {
                    ownAtt.Value = value;
                }
                else
                {
                    if (!string.IsNullOrEmpty(valueLeafName))
                    {
                        if (valueType == null)
                            throw new InvalidOperationException($"Не найден тип данных '{dataTypeName}' для атрибута '{attr.Name}'");

                        // Старый JSON может содержать значение атрибута, которого еще нет среди листьев типа.
                        // В этом случае создаем новый лист под найденным типом данных и используем его как Value.
                        var newValue = service.CreateTreeLeave(
                            valueType,
                            needAutoName: false,
                            withoutInfoNotifications: true);

                        SetImportedLeafValue(newValue, valueLeafName);
                        ownAtt.Value = newValue;

                        if (valueType != null)
                        {
                            leavesByParentUuidAndName[(valueType.Uuid, NormalizeImportName(newValue.Name))] = newValue;
                        }
                    }
                }

                refreshProgress?.Invoke(i++, count);
            }
        }

        /// <summary>
        /// Записывает импортированное значение в лист с учетом различий между системными и пользовательскими листьями.
        /// </summary>
        /// <remarks>
        /// Для системных листьев источником истины является <see cref="SystemBaseTreeLeaveModel.StringValue" />.
        /// Для пользовательских листьев импорт старого формата продолжает восстанавливать отображаемое имя.
        /// </remarks>
        /// <param name="leaf">Лист, созданный или найденный при импорте.</param>
        /// <param name="value">Импортированное строковое значение.</param>
        private static void SetImportedLeafValue(TreeLeaveModel leaf, string value)
        {
            if (leaf is SystemBaseTreeLeaveModel systemBaseLeaf)
            {
                systemBaseLeaf.StringValue = value;
                return;
            }

            leaf.Name = value;
        }

        private static Dictionary<string, ElementAttributeModel> BuildAttributesByName(IShrubMemberModel owner)
        {
            // После нормализации два разных имени из JSON могут стать одинаковыми.
            // В таком случае нельзя выбирать атрибут автоматически: импорт должен явно сообщить о конфликте.
            var duplicateNames = owner.Attributes
                .GroupBy(x => NormalizeImportName(x.Name))
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateNames.Count > 0)
            {
                throw new InvalidOperationException(
                    $"У элемента '{owner.Name}' [{owner.Uuid}] найдено несколько атрибутов с одинаковыми именами: {string.Join(", ", duplicateNames.Select(x => $"'{x}'"))}.");
            }

            return owner.Attributes.ToDictionary(x => NormalizeImportName(x.Name));
        }

        private static TreeNodeModel? ResolveNodeByName(
            string nodeName,
            IReadOnlyCollection<TreeNodeModel> allNodes,
            WorkingTreeModel preferredTree)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return null;
            }

            var normalizedNodeName = NormalizeImportName(nodeName);

            // Сначала ищем все узлы с таким именем после той же нормализации,
            // которая используется при записи Name и при поиске атрибутов в JSON.
            var candidates = allNodes
                .Where(x => NormalizeImportName(x.Name) == normalizedNodeName)
                .ToList();

            if (candidates.Count <= 1)
            {
                return candidates.SingleOrDefault();
            }

            // Если имя типа данных встречается в нескольких деревьях, предпочитаем импортируемое дерево:
            // пользовательский тип должен иметь приоритет над одноименным типом из других рабочих деревьев.
            var preferredTreeCandidates = candidates
                .Where(x => x.OwningWorkingTree?.Uuid == preferredTree.Uuid)
                .ToList();

            if (preferredTreeCandidates.Count == 1)
            {
                return preferredTreeCandidates[0];
            }

            // Если в импортируемом дереве такого типа нет, но есть ровно один системный тип,
            // используем его как стабильный fallback.
            var systemTreeCandidates = candidates
                .Where(x => x.OwningWorkingTree?.IsSystemBase == true)
                .ToList();

            if (systemTreeCandidates.Count == 1)
            {
                return systemTreeCandidates[0];
            }

            var candidateDescriptions = string.Join(", ", candidates.Select(x =>
                $"'{x.Name}' [{x.Uuid}] в дереве '{x.OwningWorkingTree?.Name}' [{x.OwningWorkingTree?.Uuid}]"));

            throw new InvalidOperationException(
                $"Найдено несколько типов данных с именем '{nodeName}', невозможно однозначно привязать атрибут. Кандидаты: {candidateDescriptions}.");
        }

        private static string NormalizeImportName(string? name)
        {
            return NameNormalizationHelper.NormalizeName(name);
        }

        private static string GetElementName(IAttributeOwnerModel element)
        {
            return element is IMainEntityModel mainEntity
                ? mainEntity.Name
                : element.GetType().Name;
        }
    }
}
