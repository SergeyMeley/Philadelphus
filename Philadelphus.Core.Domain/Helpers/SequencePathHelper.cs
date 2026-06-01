using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Помощник пути порядковых номеров.
    /// </summary>
    public static class SequencePathHelper
    {
        /// <summary>
        /// Получить путь порядковых номеров от родителя к наследнику.
        /// </summary>
        /// <param name="child">Наследник.</param>
        /// <returns>Путь порядковых номеров.</returns>
        public static string GetSequencePath(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            var items = new List<(object Item, long Sequence)>();
            var visited = new HashSet<Guid>();
            object? current = child;

            while (current != null)
            {
                if (current is ILinkableByUuidModel linkable
                    && visited.Add(linkable.Uuid) == false)
                {
                    break;
                }

                if (current is ISequencableModel sequencable
                    && sequencable.Sequence > 0)
                {
                    items.Add((current, sequencable.Sequence));
                }

                current = TryGetParent(current);
            }

            items.Reverse();
            if (items.Count > 0 && items[0].Item is not IChildrenModel)
            {
                items.RemoveAt(0);
            }

            return string.Join(".", items.Select(x => x.Sequence));
        }

        private static IParentModel? TryGetParent(object current)
        {
            if (current is not IChildrenModel currentChild)
            {
                return null;
            }

            try
            {
                return currentChild.Parent;
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }
    }
}
