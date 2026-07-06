using Philadelphus.Core.Domain.Interfaces;

using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Вспомогательные ленивые обходы для двух разных графов связей:
    /// иерархии дерева (<see cref="IParentModel" />/<see cref="IChildrenModel" />)
    /// и владения содержимым (<see cref="IOwnerModel" />/<see cref="IContentModel" />).
    /// </summary>
    /// <remarks>
    /// Публичные доменные свойства пока возвращают <see cref="ReadOnlyDictionary{TKey,TValue}" />,
    /// поэтому этот helper сначала дает iterator-обход через <c>yield return</c>, а затем при необходимости
    /// материализует результат в словарь без повторов по UUID.
    /// </remarks>
    internal static class RecursiveRelationshipHelper
    {
        /// <summary>
        /// Лениво перечисляет родителей элемента по древесной иерархии:
        /// узел или лист -> родительский узел -> корень.
        /// </summary>
        /// <remarks>
        /// Этот обход относится только к <see cref="IChildrenModel.Parent" /> и возвращает
        /// <see cref="IParentModel" />. Владельцы элемента не добавляются сюда искусственно:
        /// рабочее дерево, кустарник и репозиторий вычисляются отдельным обходом
        /// <see cref="EnumerateOwnersRecursive" />.
        /// </remarks>
        public static IEnumerable<IParentModel> EnumerateParentsRecursive(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            // Начинаем с UUID исходного элемента, чтобы цикл "элемент -> сам себе родитель"
            // не вернул исходный объект и не ушел в бесконечный обход.
            var visited = new HashSet<Guid> { child.Uuid };
            var parent = child.Parent;

            while (parent != null && visited.Add(parent.Uuid))
            {
                yield return parent;

                if (parent is not IChildrenModel parentAsChild)
                {
                    yield break;
                }

                parent = parentAsChild.Parent;
            }
        }

        /// <summary>
        /// Лениво перечисляет всех наследников по древесной иерархии:
        /// прямых детей, затем их детей и так далее.
        /// </summary>
        /// <remarks>
        /// Этот обход не смотрит на <see cref="IOwnerModel.Content" />. Например, атрибуты не являются
        /// наследниками узла в дереве; они относятся к содержимому владельца и обходятся отдельно.
        /// </remarks>
        public static IEnumerable<IChildrenModel> EnumerateChildsRecursive(IParentModel parent)
        {
            ArgumentNullException.ThrowIfNull(parent);

            var visited = new HashSet<Guid> { parent.Uuid };
            foreach (var child in EnumerateChildsRecursive(parent, visited))
            {
                yield return child;
            }
        }

        /// <summary>
        /// Лениво перечисляет владельцев содержимого:
        /// непосредственного владельца, затем владельца владельца и так далее.
        /// </summary>
        /// <remarks>
        /// Для узла цепочка владельцев идет по модели владения, а не по родителям дерева:
        /// узел -> рабочее дерево -> кустарник -> репозиторий.
        /// Родительский узел или корень остаются отдельной группой и вычисляются через
        /// <see cref="EnumerateParentsRecursive" />.
        /// </remarks>
        public static IEnumerable<IOwnerModel> EnumerateOwnersRecursive(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            var visited = new HashSet<Guid> { content.Uuid };
            var owner = content.Owner;

            while (owner != null && visited.Add(owner.Uuid))
            {
                yield return owner;

                if (owner is not IContentModel ownerAsContent)
                {
                    yield break;
                }

                owner = ownerAsContent.Owner;
            }
        }

        /// <summary>
        /// Лениво перечисляет все содержимое владельца:
        /// прямое содержимое, затем содержимое вложенных владельцев.
        /// </summary>
        /// <remarks>
        /// Этот обход не смотрит на <see cref="IParentModel.Childs" /> сам по себе. Если доменная сущность
        /// считает дочерние элементы своим содержимым, она должна явно включить их в <see cref="IOwnerModel.Content" />.
        /// Например, рабочее дерево включает корень, узлы и листья, а кустарник включает рабочие деревья.
        /// </remarks>
        public static IEnumerable<IContentModel> EnumerateContentRecursive(IOwnerModel owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            var visited = new HashSet<Guid> { owner.Uuid };
            foreach (var content in EnumerateContentRecursive(owner, visited))
            {
                yield return content;
            }
        }

        /// <summary>
        /// Материализует ленивый обход в read-only словарь, сохраняя первый встреченный объект для каждого UUID.
        /// </summary>
        public static ReadOnlyDictionary<Guid, TModel> ToReadOnlyDictionary<TModel>(IEnumerable<TModel> models)
            where TModel : ILinkableByUuidModel
        {
            ArgumentNullException.ThrowIfNull(models);

            var result = new Dictionary<Guid, TModel>();
            foreach (var model in models)
            {
                result.TryAdd(model.Uuid, model);
            }

            return result.AsReadOnly();
        }

        private static IEnumerable<IChildrenModel> EnumerateChildsRecursive(
            IParentModel parent,
            HashSet<Guid> visited)
        {
            foreach (var child in parent.Childs.Values)
            {
                // Повтор UUID означает уже посещенную ветку или цикл. Пропускаем ее целиком.
                if (visited.Add(child.Uuid) == false)
                {
                    continue;
                }

                yield return child;

                if (child is IParentModel childAsParent)
                {
                    foreach (var nestedChild in EnumerateChildsRecursive(childAsParent, visited))
                    {
                        yield return nestedChild;
                    }
                }
            }
        }

        private static IEnumerable<IContentModel> EnumerateContentRecursive(
            IOwnerModel owner,
            HashSet<Guid> visited)
        {
            foreach (var content in owner.Content.Values)
            {
                // В графе владения возможны повторные ссылки на один объект. Для индикатора достаточно
                // учесть его один раз, а повторный обход может привести к бесконечной рекурсии.
                if (visited.Add(content.Uuid) == false)
                {
                    continue;
                }

                yield return content;

                if (content is IOwnerModel contentAsOwner)
                {
                    foreach (var nestedContent in EnumerateContentRecursive(contentAsOwner, visited))
                    {
                        yield return nestedContent;
                    }
                }
            }
        }
    }
}
