using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Globalization;

namespace Philadelphus.Core.Domain.ImportExport.Mapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления DTO импорта-экспорта с доменными моделями.
    /// </summary>
    public class ImportExportDtoMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportDtoMappingProfile" />.
        /// </summary>
        public ImportExportDtoMappingProfile()
        {
            // Модель бизнес-слоя => DTO
            CreateMap<WorkingTreeModel, WorkingTreeExportDTO>()
                .ForMember(dest => dest.ContentRoot, opt => opt.MapFrom(src => src.ContentRoot));

            CreateMap<TreeRootModel, TreeRootExportDTO>()
                .ForMember(dest => dest.ChildNodes, opt => opt.MapFrom(src => src.ChildNodes))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                    src.Attributes.Where(x => x.IsRuntime == false)));

            CreateMap<TreeNodeModel, TreeNodeExportDTO>()
                .ForMember(dest => dest.OwningRootName, opt => opt.MapFrom(src => GetOwningRootName(src)))
                .ForMember(dest => dest.ChildNodes, opt => opt.MapFrom(src => src.ChildNodes))
                .ForMember(dest => dest.ChildLeaves, opt => opt.MapFrom(src => src.ChildLeaves))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                    src.Attributes.Where(x => x.IsRuntime == false)));

            CreateMap<TreeLeaveModel, TreeLeaveExportDTO>()
                .ForMember(dest => dest.OwningNodeName, opt => opt.MapFrom(src => GetOwningNodeName(src)))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                    src.Attributes.Where(x => x.IsRuntime == false)))
                .ForMember(dest => dest.ImportCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.PolymorphicParentImportCorrelationId, opt => opt.Ignore());

            CreateMap<ElementAttributeModel, AttributeExportDTO>()
                .ForMember(dest => dest.DataTypeNodeName, opt => opt.MapFrom(src => GetDataTypeNodeName(src)))
                .ForMember(dest => dest.ValueLeaveName, opt => opt.MapFrom(src => GetValueLeaveName(src)));


            // DTO => Модель бизнес-слоя
            CreateMap<WorkingTreeExportDTO, WorkingTreeModel>()
                .ConstructUsing((src, ctx) => MapWorkingTree(src, ctx))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.ContentRoot, opt => opt.Ignore())
                .ForMember(dest => dest.ContentNodes, opt => opt.Ignore())
                .ForMember(dest => dest.ContentLeaves, opt => opt.Ignore())
                .ForMember(dest => dest.ContentAttributes, opt => opt.Ignore());
        }

        private static WorkingTreeModel MapWorkingTree(WorkingTreeExportDTO source, ResolutionContext context)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(source.ContentRoot);

            var repository = context.GetImportRepository();
            var service = context.GetImportRepositoryService();
            var refreshProcess = context.GetImportRefreshProcess();
            var refreshProgress = context.GetImportRefreshProgress();

            refreshProcess("Создаем рабочее дерево");
            refreshProgress(0, 1);

            var dataStorage = repository.DataStorages?.FirstOrDefault(x => x.HasShrubMembersInfrastructureRepository)
                ?? throw new InvalidOperationException("Не найдено хранилище с поддержкой элементов рабочего дерева.");

            var tree = service.CreateWorkingTree(repository, dataStorage, needAutoName: false, withoutInfoNotifications: true);
            tree.Name = string.IsNullOrWhiteSpace(source.Name) ? "Импортированное дерево" : source.Name;

            AssignImportedSequence(
                tree,
                source.Sequence,
                repository.ContentShrub.ContentWorkingTrees
                    .Where(x => x.Uuid != tree.Uuid)
                    .Select(x => x.Sequence));

            var needRootName = string.IsNullOrWhiteSpace(source.ContentRoot.Name);
            var treeRoot = service.CreateTreeRoot(tree, needAutoName: needRootName, withoutInfoNotifications: true);
            if (needRootName == false)
            {
                treeRoot.Name = source.ContentRoot.Name;
            }

            treeRoot.Description = source.ContentRoot.Description;

            AssignImportedSequence(
                treeRoot,
                source.ContentRoot.Sequence,
                tree.ContentRoot != null && tree.ContentRoot.Uuid != treeRoot.Uuid
                    ? new[] { tree.ContentRoot.Sequence }
                    : Enumerable.Empty<long>());

            var attributeLinkMap = new Dictionary<Guid, AttributeLink>();

            refreshProgress(1, 1);

            refreshProcess("Создаем атрибуты корня");
            refreshProgress(0, 1);
            CreateAttributesFromElement(service, treeRoot, source.ContentRoot.Attributes, attributeLinkMap);
            refreshProgress(1, 1);

            refreshProcess("Создаем структуру дерева");
            refreshProgress(0, source.ContentRoot.ChildNodes.Count);

            for (var i = 0; i < source.ContentRoot.ChildNodes.Count; i++)
            {
                CreateNodeRecursive(service, treeRoot, source.ContentRoot.ChildNodes[i], attributeLinkMap);
                refreshProgress(i + 1, source.ContentRoot.ChildNodes.Count);
            }

            refreshProcess("Привязываем значения к атрибутам");
            LinkAttributesToRealEntities(service, treeRoot, attributeLinkMap, refreshProgress);

            refreshProcess("Готово");
            refreshProgress(1, 1);

            return treeRoot.OwningWorkingTree;
        }

        private static string GetOwningRootName(TreeNodeModel node)
        {
            return node.OwningWorkingTree?.ContentRoot?.Name ?? "Неизвестный";
        }

        private static void AssignImportedSequence(
            ISequencableModel model,
            long importedSequence,
            IEnumerable<long>? existSequences)
        {
            if (importedSequence > 0)
            {
                model.Sequence = importedSequence;
                if (model.Sequence == importedSequence)
                {
                    return;
                }
            }

            foreach (var sequence in SequenceHelper.GetNewSequences(existSequences ?? Enumerable.Empty<long>()))
            {
                model.Sequence = sequence;
                if (model.Sequence == sequence)
                {
                    return;
                }
            }

            throw new InvalidOperationException("Не удалось присвоить свободный порядковый номер.");
        }

        private static string GetOwningNodeName(TreeLeaveModel leaf)
        {
            return leaf.ParentNode?.Name ?? "Неизвестный";
        }

        private static string GetDataTypeNodeName(ElementAttributeModel attribute)
        {
            return attribute.ValueType?.Name ?? "Не определён";
        }

        private static string GetValueLeaveName(ElementAttributeModel attribute)
        {
            return attribute.Value?.Name ?? "Не задано";
        }

        private static T GetRequiredContextItem<T>(ResolutionContext context, string key)
        {
            if (context.Items.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            throw new InvalidOperationException($"В контексте AutoMapper не найден параметр '{key}'.");
        }

        private static void CreateNodeRecursive(
            IPhiladelphusRepositoryService service,
            IParentModel parent,
            TreeNodeExportDTO nodeDto,
            Dictionary<Guid, AttributeLink> attributeLinkMap)
        {
            var needName = string.IsNullOrWhiteSpace(nodeDto.Name);
            var node = service.CreateTreeNode(parent, needAutoName: needName, withoutInfoNotifications: true);
            if (needName == false)
            {
                node.Name = nodeDto.Name;
            }

            node.Description = nodeDto.Description;
            AssignImportedSequence(
                node,
                nodeDto.Sequence,
                parent.Childs.Values
                    .OfType<TreeNodeModel>()
                    .Where(x => x.Uuid != node.Uuid)
                    .Select(x => x.Sequence));
            CreateAttributesFromElement(service, node, nodeDto.Attributes, attributeLinkMap);

            foreach (var childNodeDto in nodeDto.ChildNodes)
            {
                CreateNodeRecursive(service, node, childNodeDto, attributeLinkMap);
            }

            foreach (var leaveDto in nodeDto.ChildLeaves)
            {
                CreateLeave(service, node, leaveDto, attributeLinkMap);
            }
        }

        private static void CreateLeave(
            IPhiladelphusRepositoryService service,
            TreeNodeModel node,
            TreeLeaveExportDTO leaveDto,
            Dictionary<Guid, AttributeLink> attributeLinkMap)
        {
            var needName = string.IsNullOrWhiteSpace(leaveDto.Name);
            var leaf = service.CreateTreeLeave(node, needAutoName: needName, withoutInfoNotifications: true);
            if (needName == false)
            {
                leaf.Name = leaveDto.Name;
            }

            leaf.Description = leaveDto.Description;
            AssignImportedSequence(
                leaf,
                leaveDto.Sequence,
                node.ChildLeaves
                    .Where(x => x.Uuid != leaf.Uuid)
                    .Select(x => x.Sequence));
            if (leaf is SystemBaseTreeLeaveModel systemBaseLeaf)
            {
                systemBaseLeaf.StringValue = string.IsNullOrEmpty(leaveDto.StringValue)
                    ? leaveDto.Name
                    : leaveDto.StringValue;
            }

            CreateAttributesFromElement(service, leaf, leaveDto.Attributes, attributeLinkMap);
        }

        private static void CreateAttributesFromElement(
            IPhiladelphusRepositoryService service,
            IAttributeOwnerModel element,
            IEnumerable<AttributeExportDTO> attributes,
            Dictionary<Guid, AttributeLink> attributeLinkMap)
        {
            foreach (var attributeDto in attributes)
            {
                var needName = string.IsNullOrWhiteSpace(attributeDto.Name);
                var normalizedName = NormalizeImportName(attributeDto.Name);

                var attribute = element.Attributes.ToList().FirstOrDefault(x => NormalizeImportName(x.Name) == normalizedName);
                if (attribute == null)
                {
                    if (element is TreeLeaveModel)
                    {
                        throw new InvalidOperationException(
                            $"Не найден унаследованный атрибут '{attributeDto.Name}' у листа '{GetElementName(element)}' [{element.Uuid}]. " +
                            "Создание собственных атрибутов для листов запрещено.");
                    }

                    attribute = service.CreateElementAttribute(element, needAutoName: needName, withoutInfoNotifications: true)
                        ?? throw new InvalidOperationException(
                            $"Не удалось создать атрибут '{attributeDto.Name}' для элемента '{GetElementName(element)}' [{element.Uuid}].");

                    if (needName == false)
                    {
                        attribute.Name = attributeDto.Name;
                    }
                }

                attribute.IsCollectionValue = attributeDto.IsCollectionValue;
                if (attributeDto.Visibility != VisibilityScope.SystemError)
                {
                    attribute.Visibility = attributeDto.Visibility;
                }

                if (attributeDto.Override != OverrideType.SystemError)
                {
                    attribute.Override = attributeDto.Override;
                }

                attribute.Description = attributeDto.Description;
                AssignImportedSequence(
                    attribute,
                    attributeDto.Sequence,
                    element.Attributes
                        .Where(x => x.Uuid != attribute.Uuid)
                        .Select(x => x.Sequence));

                attributeLinkMap[attribute.LocalUuid] = new AttributeLink(
                    GetDataTypeNodeName(attributeDto),
                    GetValueLeafName(attributeDto),
                    attribute);
            }
        }

        private static void LinkAttributesToRealEntities(
            IPhiladelphusRepositoryService service,
            TreeRootModel treeRoot,
            Dictionary<Guid, AttributeLink> attributeLinkMap,
            Action<int, int> refreshProgress)
        {
            var allNodes = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentNodes ?? Enumerable.Empty<TreeNodeModel>())
                .ToList();

            var allLeaves = treeRoot.OwningShrub.ContentWorkingTrees
                .SelectMany(x => x.ContentLeaves ?? Enumerable.Empty<TreeLeaveModel>())
                .ToList();

            var leavesByParentUuidAndName = allLeaves
                .GroupBy(x => (ParentUuid: x.ParentNode.Uuid, LeafName: NormalizeImportName(x.Name)))
                .ToDictionary(x => x.Key, x => x.First());

            var attributesByOwner = new Dictionary<IShrubMemberModel, Dictionary<string, ElementAttributeModel>>();
            var count = attributeLinkMap.Count;
            var current = 0;

            foreach (var link in attributeLinkMap.Values)
            {
                if (link.Attribute.Owner is not IShrubMemberModel owner)
                {
                    throw new InvalidOperationException($"Не найден владелец атрибута '{link.Attribute.Name}'.");
                }

                if (attributesByOwner.TryGetValue(owner, out var attributesByName) == false)
                {
                    attributesByName = BuildAttributesByName(owner);
                    attributesByOwner[owner] = attributesByName;
                }

                if (attributesByName.TryGetValue(NormalizeImportName(link.Attribute.Name), out var ownAttribute) == false)
                {
                    throw new InvalidOperationException(
                        $"Не найден атрибут '{link.Attribute.Name}' у элемента '{owner.Name}' [{owner.Uuid}].");
                }

                var valueType = ResolveNodeByName(link.DataTypeName, allNodes, treeRoot.OwningWorkingTree);
                ownAttribute.ValueType = valueType;

                var valueLeafName = NormalizeSystemBaseImportValue(valueType, link.ValueLeafName);

                if (valueType != null
                    && leavesByParentUuidAndName.TryGetValue((valueType.Uuid, NormalizeImportName(valueLeafName)), out var value))
                {
                    ownAttribute.Value = value;
                }
                else if (string.IsNullOrWhiteSpace(valueLeafName) == false)
                {
                    if (valueType == null)
                    {
                        throw new InvalidOperationException(
                            $"Не найден тип данных '{link.DataTypeName}' для атрибута '{link.Attribute.Name}'.");
                    }

                    if (valueType is SystemBaseTreeNodeModel
                        && ownAttribute.IsCollectionValue == false)
                    {
                        ownAttribute.TrySetSystemBaseValueFromString(valueLeafName);
                        refreshProgress(++current, count);
                        continue;
                    }

                    var newValue = service.CreateTreeLeave(valueType, needAutoName: false, withoutInfoNotifications: true);
                    SetImportedLeafValue(newValue, valueLeafName);
                    ownAttribute.Value = newValue;
                    leavesByParentUuidAndName[(valueType.Uuid, NormalizeImportName(newValue.Name))] = newValue;
                }

                refreshProgress(++current, count);
            }
        }

        private static Dictionary<string, ElementAttributeModel> BuildAttributesByName(IShrubMemberModel owner)
        {
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
            var candidates = allNodes
                .Where(x => NormalizeImportName(x.Name) == normalizedNodeName)
                .ToList();

            if (candidates.Count <= 1)
            {
                return candidates.SingleOrDefault();
            }

            var preferredTreeCandidates = candidates
                .Where(x => x.OwningWorkingTree?.Uuid == preferredTree.Uuid)
                .ToList();
            if (preferredTreeCandidates.Count == 1)
            {
                return preferredTreeCandidates[0];
            }

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

        private static void SetImportedLeafValue(TreeLeaveModel leaf, string value)
        {
            if (leaf is SystemBaseTreeLeaveModel systemBaseLeaf)
            {
                systemBaseLeaf.StringValue = value;
                return;
            }

            leaf.Name = value;
        }

        private static string GetDataTypeNodeName(AttributeExportDTO attributeDto)
        {
            var dataTypeNodeName = string.IsNullOrWhiteSpace(attributeDto.DataTypeNodeName)
                || attributeDto.DataTypeNodeName == "Не определён"
                    ? "Текст"
                    : attributeDto.DataTypeNodeName;

            return dataTypeNodeName == "Строка"
                ? "Текст"
                : dataTypeNodeName;
        }

        private static string GetValueLeafName(AttributeExportDTO attributeDto)
        {
            return string.IsNullOrWhiteSpace(attributeDto.ValueLeaveName)
                || attributeDto.ValueLeaveName == "Не задано"
                    ? string.Empty
                    : attributeDto.ValueLeaveName;
        }

        /// <summary>
        /// Приводит строковое значение системного типа к формату, который принимает доменная валидация.
        /// </summary>
        /// <param name="valueType">Тип данных атрибута.</param>
        /// <param name="valueLeafName">Импортированное строковое значение.</param>
        /// <returns>Нормализованное значение.</returns>
        private static string NormalizeSystemBaseImportValue(TreeNodeModel? valueType, string valueLeafName)
        {
            if (valueType is not SystemBaseTreeNodeModel systemBaseNode
                || string.IsNullOrWhiteSpace(valueLeafName))
            {
                return valueLeafName;
            }

            var trimmedValue = valueLeafName.Trim();
            switch (systemBaseNode.SystemBaseType)
            {
                case SystemBaseType.INTEGER:
                    return long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out var integerValue)
                        || long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerValue)
                            ? integerValue.ToString(CultureInfo.InvariantCulture)
                            : valueLeafName;
                case SystemBaseType.NUMERIC:
                case SystemBaseType.FLOAT:
                    return double.TryParse(trimmedValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var doubleValue)
                        || double.TryParse(trimmedValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out doubleValue)
                            ? doubleValue.ToString(CultureInfo.InvariantCulture)
                            : valueLeafName;
                case SystemBaseType.MONEY:
                    return decimal.TryParse(trimmedValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var decimalValue)
                        || decimal.TryParse(trimmedValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimalValue)
                            ? decimalValue.ToString(CultureInfo.InvariantCulture)
                            : valueLeafName;
                default:
                    return valueLeafName;
            }
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

        private readonly record struct AttributeLink(
            string DataTypeName,
            string ValueLeafName,
            ElementAttributeModel Attribute);
    }
}
