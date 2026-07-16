using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Models.Tables;
using Philadelphus.Presentation.Services.StateVisibility;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Philadelphus.Presentation.Services.Tables
{
    /// <summary>
    /// Строит чистые модели Excel-подобной таблицы наследников выбранного элемента.
    /// </summary>
    /// <remarks>
    /// Сервис не создает WPF-контролы и не хранит UI-состояние. Его результат можно тестировать
    /// отдельно от представления и переиспользовать в других presentation-клиентах.
    /// </remarks>
    public static class ChildCollectionTableBuilder
    {
        /// <summary>
        /// Возвращает всех наследников выбранного элемента в depth-first порядке.
        /// </summary>
        /// <remarks>
        /// Дети каждого уровня сортируются по Sequence, имени и Uuid. Поэтому наследники n+1 порядка
        /// идут сразу под своим родителем n порядка.
        /// </remarks>
        public static IReadOnlyList<IChildrenModel> buildChildCollectionTableChildren(IParentModel? currentElement)
        {
            if (currentElement == null)
            {
                return Array.Empty<IChildrenModel>();
            }

            var result = new List<IChildrenModel>();
            var visited = new HashSet<Guid>();

            AddDescendantsDepthFirst(currentElement, result, visited);

            return result;
        }

        /// <summary>
        /// Формирует стабильный набор колонок: состояние, системные свойства, видимые детям атрибуты и audit-поля.
        /// </summary>
        /// <remarks>
        /// Динамические атрибуты берутся только с текущего выбранного элемента и проходят проверку Visibility
        /// относительно фактических наследников. Пользовательское имя атрибута остается логическим ключом,
        /// а для WPF binding-а создается отдельный безопасный BindingKey.
        /// </remarks>
        public static IReadOnlyList<ChildCollectionTableColumn> buildChildCollectionTableColumns(
            IAttributeOwnerModel? currentElement,
            IEnumerable<IChildrenModel>? children)
        {
            var result = new List<ChildCollectionTableColumn>
            {
                CreateColumn(
                    nameof(IMainEntityModel.State),
                    0,
                    child => child is IMainEntityModel entity ? entity.State : null,
                    typeof(IMainEntityModel),
                    nameof(IMainEntityModel.State)),
                CreateColumn(
                    nameof(IChildrenModel.SequencePath),
                    1,
                    child => NullIfEmpty(child.SequencePath),
                    typeof(IChildrenModel),
                    nameof(IChildrenModel.SequencePath),
                    isReadOnly: false,
                    setterFactory: GetSequencePathSetter),
                CreateColumn(
                    nameof(IMainEntityModel.Type),
                    2,
                    child => child is IMainEntityModel entity ? entity.Type : child.GetType().Name,
                    typeof(IMainEntityModel),
                    nameof(IMainEntityModel.Type)),
                CreateColumn(
                    nameof(IMainEntityModel.Name),
                    3,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.Name) : null,
                    typeof(IMainEntityModel),
                    nameof(IMainEntityModel.Name),
                    isReadOnly: false,
                    setterFactory: GetNameSetter),
                CreateColumn(
                    nameof(IMainEntityModel.Description),
                    4,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.Description) : null,
                    typeof(IMainEntityModel),
                    nameof(IMainEntityModel.Description),
                    isReadOnly: false,
                    setterFactory: GetDescriptionSetter),
                CreateColumn(
                    nameof(IWorkingTreeMemberModel.CustomCode),
                    5,
                    GetCustomCode,
                    typeof(IWorkingTreeMemberModel),
                    nameof(IWorkingTreeMemberModel.CustomCode),
                    isReadOnly: false,
                    setterFactory: GetCustomCodeSetter),
            };

            var order = 6;
            var attributes = GetTableAttributes(currentElement, children).ToList();
            var reservedBindingKeys = new HashSet<string>(
                result.Select(x => x.Key).Concat(attributes.Select(GetAttributeHeader)),
                StringComparer.Ordinal);

            foreach (var attribute in attributes)
            {
                var header = GetAttributeHeader(attribute);
                var bindingKey = CreateAttributeBindingKey(order, reservedBindingKeys);

                result.Add(new ChildCollectionTableColumn(
                    header,
                    header,
                    order++,
                    child => GetAttributeValue(child, header),
                    isReadOnly: attribute.IsCollectionValue,
                    isAttribute: true,
                    setterFactory: child => GetAttributeValueSetter(child, header),
                    valueOptionsGetter: attribute.IsCollectionValue
                        ? null
                        : child => GetAttributeValueOptions(child, header),
                    bindingKey: bindingKey));
            }

            result.AddRange(new[]
            {
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.CreatedBy) : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.CreatedBy)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedAt)}",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfDefault(entity.AuditInfo?.CreatedAt) : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.CreatedAt)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.UpdatedBy)}",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.UpdatedBy) : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.UpdatedBy)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.UpdatedAt)}",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.UpdatedAt : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.UpdatedAt)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.ContentUpdatedBy)}",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.ContentUpdatedBy) : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.ContentUpdatedBy)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.ContentUpdatedAt)}",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.ContentUpdatedAt : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.ContentUpdatedAt)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.IsDeleted)}",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.IsDeleted : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.IsDeleted)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.DeletedBy)}",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.DeletedBy) : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.DeletedBy)),
                CreateColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.DeletedAt)}",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.DeletedAt : null,
                    typeof(AuditInfoModel),
                    nameof(AuditInfoModel.DeletedAt)),
            });

            return result.OrderBy(x => x.Order).ToList();
        }

        /// <summary>
        /// Создает колонку свойства модели с заголовком и tooltip из DisplayAttribute, если он задан.
        /// </summary>
        private static ChildCollectionTableColumn CreateColumn(
            string key,
            int order,
            Func<IChildrenModel, object?> valueGetter,
            Type displayType,
            string displayPropertyName,
            bool isReadOnly = true,
            bool isAttribute = false,
            Func<IChildrenModel, Func<object?, object?>?>? setterFactory = null,
            Func<IChildrenModel, IEnumerable<object>?>? valueOptionsGetter = null,
            string? bindingKey = null)
        {
            var display = GetPropertyDisplay(displayType, displayPropertyName);

            return new ChildCollectionTableColumn(
                key,
                display.Name,
                order,
                valueGetter,
                isReadOnly,
                isAttribute,
                setterFactory,
                valueOptionsGetter,
                bindingKey,
                display.Description);
        }

        /// <summary>
        /// Ищет DisplayAttribute у свойства указанного типа или его реализаций.
        /// </summary>
        private static (string Name, string? Description) GetPropertyDisplay(Type type, string propertyName)
        {
            if (TryGetPropertyDisplay(type, propertyName, out var display))
            {
                return display;
            }

            foreach (var candidateType in type.Assembly.GetTypes()
                .Where(x => x != type && type.IsAssignableFrom(x)))
            {
                if (TryGetPropertyDisplay(candidateType, propertyName, out display))
                {
                    return display;
                }
            }

            return (propertyName, null);
        }

        private static bool TryGetPropertyDisplay(
            Type type,
            string propertyName,
            out (string Name, string? Description) display)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            var attribute = property?.GetCustomAttribute<DisplayAttribute>();

            if (attribute == null)
            {
                display = default;
                return false;
            }

            var name = string.IsNullOrWhiteSpace(attribute?.Name)
                ? propertyName
                : attribute.Name!;
            var description = string.IsNullOrWhiteSpace(attribute?.Description)
                ? null
                : attribute.Description;

            display = (name, description);
            return true;
        }

        /// <summary>
        /// Формирует строки таблицы и связывает их с исходными доменными моделями.
        /// </summary>
        /// <remarks>
        /// Ячейки, setter-ы и refresh-делегаты индексируются BindingKey. Одновременно создается карта
        /// логических ключей в BindingKey, чтобы уведомления и тесты оставались привязаны к именам свойств
        /// и атрибутов, а не к техническим ключам WPF.
        /// </remarks>
        public static IReadOnlyList<ChildCollectionTableRow> buildChildCollectionTableRows(
            IEnumerable<IChildrenModel>? children,
            IEnumerable<ChildCollectionTableColumn>? columns,
            Action<Guid, string>? cellChanged = null)
        {
            var columnsList = columns?.OrderBy(x => x.Order).ToList()
                ?? new List<ChildCollectionTableColumn>();

            if (children == null)
            {
                return Array.Empty<ChildCollectionTableRow>();
            }

            var rows = new List<ChildCollectionTableRow>();
            foreach (var child in children)
            {
                var cells = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => x.GetValue(child));

                var setters = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => x.GetSetter(child));

                var valueOptions = columnsList
                    .Where(x => x.HasValueOptions)
                    .ToDictionary(
                        x => x.BindingKey,
                        x => x.GetValueOptions(child));

                var refreshers = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => new Func<object?>(() => x.GetValue(child)));

                var valueOverrideStates = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => IsAttributeValueOverridden(child, x));

                var valueOverrideStateRefreshers = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => new Func<bool>(() => IsAttributeValueOverridden(child, x)));

                var valueOverrideToolTips = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => GetAttributeValueOverrideToolTip(child, x));

                var valueOverrideToolTipRefreshers = columnsList.ToDictionary(
                    x => x.BindingKey,
                    x => new Func<string?>(() => GetAttributeValueOverrideToolTip(child, x)));

                // Каналы «отображаемый текст» и «текст редактирования» только для редактируемых
                // одиночных атрибутных колонок (HasValueOptions) — чтобы ячейка значения работала
                // идентично таблице атрибутов (формула / ссылка / системное значение / код ошибки).
                var attributeValueColumns = columnsList
                    .Where(x => x.IsAttribute && x.HasValueOptions)
                    .ToList();

                var displayTexts = attributeValueColumns.ToDictionary(
                    x => x.BindingKey,
                    x => GetAttributeDisplayText(child, x.Key));

                var displayTextRefreshers = attributeValueColumns.ToDictionary(
                    x => x.BindingKey,
                    x => new Func<string?>(() => GetAttributeDisplayText(child, x.Key)));

                var editTextGetters = attributeValueColumns.ToDictionary(
                    x => x.BindingKey,
                    x => new Func<string?>(() => GetAttributeEditText(child, x.Key)));

                var editTextSetters = attributeValueColumns.ToDictionary(
                    x => x.BindingKey,
                    x => new Action<string?>(value => SetAttributeEditText(child, x.Key, value)));

                var keyAliases = columnsList.ToDictionary(
                    x => x.Key,
                    x => x.BindingKey);

                rows.Add(new ChildCollectionTableRow(
                    cells,
                    setters,
                    valueOptions,
                    refreshers,
                    keyAliases,
                    child.Uuid,
                    cellChanged,
                    valueOverrideStates,
                    valueOverrideStateRefreshers,
                    valueOverrideToolTips,
                    valueOverrideToolTipRefreshers,
                    displayTexts,
                    displayTextRefreshers,
                    editTextGetters,
                    editTextSetters,
                    () => GetStateVisibilityInfo(child).ParentOwnerState ?? State.SavedOrLoaded,
                    () => child is IMainEntityModel entity ? entity.State : State.SavedOrLoaded,
                    () => GetStateVisibilityInfo(child).ChildContentState ?? State.SavedOrLoaded,
                    () => child is IMainEntityModel entity ? StateVisibilityInfoBuilder.Build(entity).ToolTip : string.Empty));
            }

            return rows;
        }

        private static StateVisibilityInfo GetStateVisibilityInfo(IChildrenModel child)
            => child is IMainEntityModel entity
                ? StateVisibilityInfoBuilder.Build(entity)
                : new StateVisibilityInfo(null, State.SavedOrLoaded, null, string.Empty);

        /// <summary>
        /// Рекурсивно добавляет наследников так, чтобы дочерние элементы располагались сразу под родителем.
        /// </summary>
        private static void AddDescendantsDepthFirst(
            IParentModel parent,
            ICollection<IChildrenModel> result,
            ISet<Guid> visited)
        {
            foreach (var child in GetSortedChildren(parent))
            {
                if (visited.Add(child.Uuid) == false)
                {
                    continue;
                }

                result.Add(child);

                if (child is IParentModel childParent)
                {
                    AddDescendantsDepthFirst(childParent, result, visited);
                }
            }
        }

        private static IEnumerable<IChildrenModel> GetSortedChildren(IParentModel parent)
        {
            return parent.Childs?.Values
                .OrderBy(x => x is ISequencableModel sequencable ? sequencable.Sequence : long.MaxValue)
                .ThenBy(x => x is IMainEntityModel entity ? entity.Name : x.GetType().Name)
                .ThenBy(x => x.Uuid)
                ?? Enumerable.Empty<IChildrenModel>();
        }

        /// <summary>
        /// Возвращает атрибуты текущего элемента, которые хотя бы один наследник может видеть по Visibility.
        /// </summary>
        private static IEnumerable<ElementAttributeModel> GetTableAttributes(
            IAttributeOwnerModel? currentElement,
            IEnumerable<IChildrenModel>? children)
        {
            var attributesByName = new Dictionary<string, ElementAttributeModel>(StringComparer.Ordinal);
            var childrenList = children?.ToList() ?? new List<IChildrenModel>();

            AddVisibleAttributesForChildren(
                attributesByName,
                currentElement,
                childrenList);

            return attributesByName.Values
                .OrderBy(x => x.DeclaringOwner is ISequencableModel sequencable ? sequencable.Sequence : long.MaxValue)
                .ThenBy(x => x.InheritanceDepth)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.DeclaringUuid);
        }

        private static void AddVisibleAttributesForChildren(
            IDictionary<string, ElementAttributeModel> attributesByName,
            IAttributeOwnerModel? currentElement,
            IReadOnlyList<IChildrenModel> children)
        {
            if (currentElement?.Attributes == null || children.Count == 0)
            {
                return;
            }

            foreach (var attribute in currentElement.Attributes)
            {
                if (attribute.DeclaringUuid == Guid.Empty)
                {
                    continue;
                }

                if (CanAnyChildSeeAttribute(currentElement, attribute, children))
                {
                    attributesByName.TryAdd(GetAttributeHeader(attribute), attribute);
                }
            }
        }

        /// <summary>
        /// Проверяет, доступен ли атрибут текущего элемента хотя бы одному наследнику таблицы.
        /// </summary>
        private static bool CanAnyChildSeeAttribute(
            IAttributeOwnerModel currentElement,
            ElementAttributeModel attribute,
            IEnumerable<IChildrenModel> children)
        {
            return attribute.Visibility switch
            {
                VisibilityScope.Public => true,
                VisibilityScope.Internal => children.Any(child => child is IWorkingTreeMemberModel),
                VisibilityScope.Protected => children.Any(child => IsDescendantOrPrivateLeaf(currentElement, child)),
                VisibilityScope.InternalProtected => children.Any(child => child is IWorkingTreeMemberModel || IsDescendantOrPrivateLeaf(currentElement, child)),
                VisibilityScope.Private => children.Any(child => IsPrivateVisibleToChild(currentElement, child)),
                _ => false,
            };
        }

        private static bool IsDescendantOrPrivateLeaf(
            IAttributeOwnerModel currentElement,
            IChildrenModel child)
        {
            return child.Parent == currentElement
                || child.Parent is IChildrenModel parentChild && IsDescendantOrPrivateLeaf(currentElement, parentChild);
        }

        private static bool IsPrivateVisibleToChild(
            IAttributeOwnerModel currentElement,
            IChildrenModel child)
        {
            return currentElement is TreeNodeModel node
                && child is TreeLeaveModel leave
                && leave.ParentNode.Uuid == node.Uuid;
        }

        private static Func<object?, object?>? GetSequencePathSetter(IChildrenModel child)
        {
            if (child is not ISequencableModel sequencable
                || IsWritableProperty(child, nameof(ISequencableModel.Sequence)) == false)
            {
                return null;
            }

            return value =>
            {
                if (TryConvertSequencePathOwnPart(value, out var sequence) == false)
                {
                    return NullIfEmpty(child.SequencePath);
                }

                sequencable.Sequence = sequence;
                return NullIfEmpty(child.SequencePath);
            };
        }

        private static Func<object?, object?>? GetNameSetter(IChildrenModel child)
        {
            if (child is not IMainEntityModel entity
                || IsWritableProperty(child, nameof(IMainEntityModel.Name)) == false)
            {
                return null;
            }

            return value =>
            {
                entity.Name = ConvertToString(value);
                return NullIfEmpty(entity.Name);
            };
        }

        private static Func<object?, object?>? GetDescriptionSetter(IChildrenModel child)
        {
            if (child is not IMainEntityModel entity
                || IsWritableProperty(child, nameof(IMainEntityModel.Description)) == false)
            {
                return null;
            }

            return value =>
            {
                entity.Description = ConvertToString(value);
                return NullIfEmpty(entity.Description);
            };
        }

        private static Func<object?, object?>? GetCustomCodeSetter(IChildrenModel child)
        {
            return child switch
            {
                TreeRootModel root when IsWritableProperty(root, nameof(TreeRootModel.CustomCode)) => value =>
                {
                    root.CustomCode = ConvertToString(value);
                    return GetCustomCode(child);
                },
                TreeNodeModel node when IsWritableProperty(node, nameof(TreeNodeModel.CustomCode)) => value =>
                {
                    node.CustomCode = ConvertToString(value);
                    return GetCustomCode(child);
                },
                TreeLeaveModel leave when IsWritableProperty(leave, nameof(TreeLeaveModel.CustomCode)) => value =>
                {
                    leave.CustomCode = ConvertToString(value);
                    return GetCustomCode(child);
                },
                _ => null,
            };
        }

        private static bool IsWritableProperty(object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName)?.SetMethod?.IsPublic == true;
        }

        private static string ConvertToString(object? value)
        {
            return value?.ToString() ?? string.Empty;
        }

        private static bool TryConvertToInt64(object? value, out long result)
        {
            if (value is long longValue)
            {
                result = longValue;
                return true;
            }

            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                result = 0;
                return true;
            }

            return long.TryParse(value.ToString(), out result);
        }

        private static bool TryConvertSequencePathOwnPart(object? value, out long result)
        {
            if (value is long longValue)
            {
                result = longValue;
                return true;
            }

            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                result = 0;
                return true;
            }

            var ownPart = text.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault();

            return long.TryParse(ownPart, out result);
        }

        private static string? GetCustomCode(IChildrenModel child)
        {
            return child switch
            {
                TreeRootModel root => NullIfEmpty(root.CustomCode),
                TreeNodeModel node => NullIfEmpty(node.CustomCode),
                TreeLeaveModel leave => NullIfEmpty(leave.CustomCode),
                _ => null,
            };
        }

        private static object? GetAttributeValue(IChildrenModel child, string attributeName)
        {
            var attribute = GetAttribute(child, attributeName);
            if (attribute == null)
            {
                return null;
            }

            if (attribute.IsCollectionValue)
            {
                var values = attribute.Values
                    .Select(x => x.Name)
                    .Where(x => string.IsNullOrWhiteSpace(x) == false)
                    .ToList();
                if (string.IsNullOrWhiteSpace(attribute.ValuesReferenceErrorCode) == false)
                {
                    values.Add(attribute.ValuesReferenceErrorCode);
                }

                return values.Count == 0 ? null : string.Join("; ", values);
            }

            if (string.IsNullOrWhiteSpace(attribute.ValueFormula) == false
                && string.IsNullOrWhiteSpace(attribute.ValueFormulaErrorCode) == false)
            {
                return attribute.ValueFormulaErrorCode;
            }

            return attribute.Value;
        }

        /// <summary>
        /// Определяет, переопределено ли значение атрибута для строки таблицы наследников.
        /// </summary>
        private static bool IsAttributeValueOverridden(IChildrenModel child, ChildCollectionTableColumn column)
        {
            if (column.IsAttribute == false)
            {
                return false;
            }

            var attribute = GetAttribute(child, column.Key);
            if (attribute == null)
            {
                return false;
            }

            return attribute.IsCollectionValue
                ? attribute.AreValuesOverridden
                : attribute.IsValueOverridden;
        }

        /// <summary>
        /// Возвращает подсказку для переопределенного значения атрибута в таблице наследников.
        /// </summary>
        private static string? GetAttributeValueOverrideToolTip(IChildrenModel child, ChildCollectionTableColumn column)
        {
            if (IsAttributeValueOverridden(child, column) == false)
            {
                return null;
            }

            var attribute = GetAttribute(child, column.Key);
            if (attribute == null)
            {
                return null;
            }

            var parentValue = attribute.IsCollectionValue
                ? FormatValues(attribute.InheritedAttributeFromParent?.Values)
                : FormatValue(attribute.InheritedAttributeFromParent?.Value);

            return $"Значение атрибута переопределено относительно родительского атрибута. У родителя: {parentValue}";
        }

        private static string FormatValue(TreeLeaveModel? value)
        {
            return string.IsNullOrWhiteSpace(value?.Name) ? "<не задано>" : value.Name;
        }

        private static string FormatValues(IEnumerable<TreeLeaveModel>? values)
        {
            var names = values?
                .Select(x => x.Name)
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .ToArray();

            return names is { Length: > 0 }
                ? string.Join("; ", names)
                : "<не задано>";
        }

        private static Func<object?, object?>? GetAttributeValueSetter(IChildrenModel child, string attributeName)
        {
            var attribute = GetAttribute(child, attributeName);
            if (attribute == null || attribute.IsCollectionValue)
            {
                return null;
            }

            return value =>
            {
                if (value is string stringValue
                    && attribute.TrySetSystemBaseValueAsFormula(stringValue))
                {
                    return attribute.Value;
                }

                if (value is not TreeLeaveModel treeLeave)
                {
                    return attribute.Value;
                }

                attribute.AssignValueAsFormula(treeLeave);
                return attribute.Value;
            };
        }

        private static IEnumerable<object>? GetAttributeValueOptions(IChildrenModel child, string attributeName)
        {
            return GetAttribute(child, attributeName)?.ValuesList?.Cast<object>();
        }

        /// <summary>
        /// Отображаемый текст значения атрибута (результат формулы / код ошибки / значение) —
        /// идентично ячейке таблицы атрибутов.
        /// </summary>
        private static string? GetAttributeDisplayText(IChildrenModel child, string attributeName)
        {
            var attribute = GetAttribute(child, attributeName);
            return attribute == null
                ? null
                : AttributeValueText.GetDisplayText(attribute);
        }

        /// <summary>
        /// Текст редактирования значения атрибута (формула / ссылка «[uuid]»).
        /// </summary>
        private static string? GetAttributeEditText(IChildrenModel child, string attributeName)
        {
            var attribute = GetAttribute(child, attributeName);
            return attribute == null
                ? null
                : AttributeValueText.GetFormulaText(attribute);
        }

        /// <summary>
        /// Применяет введённый в ячейку текст значения атрибута к доменной модели (формула / ссылка /
        /// системное значение) — идентично сеттеру ячейки таблицы атрибутов.
        /// </summary>
        private static void SetAttributeEditText(IChildrenModel child, string attributeName, string? value)
        {
            var attribute = GetAttribute(child, attributeName);
            if (attribute == null || attribute.IsCollectionValue)
            {
                return;
            }

            attribute.SetFormulaText(value);
        }

        private static ElementAttributeModel? GetAttribute(IChildrenModel child, string attributeName)
        {
            if (child is not IAttributeOwnerModel attributeOwner)
            {
                return null;
            }

            return attributeOwner.Attributes?
                .FirstOrDefault(x => string.Equals(x.Name, attributeName, StringComparison.Ordinal));
        }

        private static string GetAttributeHeader(ElementAttributeModel attribute)
        {
            return string.IsNullOrWhiteSpace(attribute.Name)
                ? attribute.DeclaringUuid.ToString()
                : attribute.Name;
        }

        /// <summary>
        /// Создает безопасный технический ключ для динамической колонки атрибута.
        /// </summary>
        /// <remarks>
        /// Ключ не строится из имени атрибута, потому что имя является пользовательскими данными
        /// и может ломать WPF binding path. Дополнительно проверяется отсутствие коллизий с логическими ключами.
        /// </remarks>
        private static string CreateAttributeBindingKey(int order, ISet<string> reservedBindingKeys)
        {
            var index = order;
            string bindingKey;
            do
            {
                bindingKey = $"attribute_{index++}";
            }
            while (reservedBindingKeys.Add(bindingKey) == false);

            return bindingKey;
        }

        private static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        private static DateTime? NullIfDefault(DateTime? value)
        {
            return value.HasValue && value.Value != default
                ? value.Value
                : null;
        }
    }
}
