using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Models.Tables;

namespace Philadelphus.Presentation.Wpf.UI.Services.Tables
{
    /// <summary>
    /// Builds pure table descriptors for children of a selected repository member.
    /// </summary>
    public static class ChildCollectionTableBuilder
    {
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

        public static IReadOnlyList<ChildCollectionTableColumn> buildChildCollectionTableColumns(
            IAttributeOwnerModel? currentElement,
            IEnumerable<IChildrenModel>? children)
        {
            var result = new List<ChildCollectionTableColumn>
            {
                new(
                    nameof(IMainEntityModel.State),
                    "State",
                    0,
                    child => child is IMainEntityModel entity ? entity.State : null),
                new(
                    nameof(ISequencableModel.Sequence),
                    "№",
                    1,
                    child => child is ISequencableModel sequencable ? sequencable.Sequence : null,
                    isReadOnly: false,
                    setterFactory: GetSequenceSetter),
                new(
                    nameof(IMainEntityModel.Type),
                    "Тип",
                    2,
                    child => child is IMainEntityModel entity ? entity.Type : child.GetType().Name),
                new(
                    nameof(IMainEntityModel.Name),
                    "Name",
                    3,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.Name) : null,
                    isReadOnly: false,
                    setterFactory: GetNameSetter),
                new(
                    nameof(IMainEntityModel.Description),
                    "Description",
                    4,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.Description) : null,
                    isReadOnly: false,
                    setterFactory: GetDescriptionSetter),
                new(
                    nameof(IWorkingTreeMemberModel.CustomCode),
                    "CustomCode",
                    5,
                    GetCustomCode,
                    isReadOnly: false,
                    setterFactory: GetCustomCodeSetter),
            };

            var order = 6;
            foreach (var attribute in GetTableAttributes(currentElement, children))
            {
                var header = GetAttributeHeader(attribute);
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
                        : child => GetAttributeValueOptions(child, header)));
            }

            result.AddRange(new[]
            {
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}",
                    "CreatedBy",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.CreatedBy) : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedAt)}",
                    "CreatedAt",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfDefault(entity.AuditInfo?.CreatedAt) : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.UpdatedBy)}",
                    "UpdatedBy",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.UpdatedBy) : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.UpdatedAt)}",
                    "UpdatedAt",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.UpdatedAt : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.ContentUpdatedBy)}",
                    "ContentUpdatedBy",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.ContentUpdatedBy) : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.ContentUpdatedAt)}",
                    "ContentUpdatedAt",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.ContentUpdatedAt : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.IsDeleted)}",
                    "IsDeleted",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.IsDeleted : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.DeletedBy)}",
                    "DeletedBy",
                    order++,
                    child => child is IMainEntityModel entity ? NullIfEmpty(entity.AuditInfo?.DeletedBy) : null),
                new ChildCollectionTableColumn(
                    $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.DeletedAt)}",
                    "DeletedAt",
                    order++,
                    child => child is IMainEntityModel entity ? entity.AuditInfo?.DeletedAt : null),
            });

            return result.OrderBy(x => x.Order).ToList();
        }

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
                    x => x.Key,
                    x => x.GetValue(child));

                var setters = columnsList.ToDictionary(
                    x => x.Key,
                    x => x.GetSetter(child));

                var valueOptions = columnsList
                    .Where(x => x.HasValueOptions)
                    .ToDictionary(
                        x => x.Key,
                        x => x.GetValueOptions(child));

                var refreshers = columnsList.ToDictionary(
                    x => x.Key,
                    x => new Func<object?>(() => x.GetValue(child)));

                rows.Add(new ChildCollectionTableRow(cells, setters, valueOptions, refreshers, child.Uuid, cellChanged));
            }

            return rows;
        }

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

        private static Func<object?, object?>? GetSequenceSetter(IChildrenModel child)
        {
            if (child is not ISequencableModel sequencable
                || IsWritableProperty(child, nameof(ISequencableModel.Sequence)) == false)
            {
                return null;
            }

            return value =>
            {
                if (TryConvertToInt64(value, out var sequence) == false)
                {
                    return sequencable.Sequence;
                }

                sequencable.Sequence = sequence;
                return sequencable.Sequence;
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
                return attribute.Values == null || attribute.Values.Count == 0
                    ? null
                    : string.Join("; ", attribute.Values.Select(x => x.Name).Where(x => string.IsNullOrWhiteSpace(x) == false));
            }

            return attribute.Value;
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
                attribute.Value = value as TreeLeaveModel;
                return attribute.Value;
            };
        }

        private static IEnumerable<object>? GetAttributeValueOptions(IChildrenModel child, string attributeName)
        {
            return GetAttribute(child, attributeName)?.ValuesList?.Cast<object>();
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
