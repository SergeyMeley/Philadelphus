using System.Buffers;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Помощник наименования
    /// </summary>
    public static class NamingHelper
    {
        private static readonly object SyncRoot = new();
        private static readonly HashSet<string> _existNames = new();
        private static readonly Dictionary<string, int> _indexesByFixedParts = new();

        /// <summary>
        /// Использованные (занятые) наименования
        /// </summary>
        public static ReadOnlySet<string> ExistNames
        {
            get
            {
                lock (SyncRoot)
                {
                    return _existNames.ToHashSet().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Получить новое наименование
        /// </summary>
        /// <param name="fixedPartOfName">Фиксированная часть наименования</param>
        /// <param name="existNames">Коллекция занятых наименований</param>
        /// <returns>Результат выполнения операции.</returns>
        public static string GetNewName(string fixedPartOfName = "Новое наименование")
        {
            lock (SyncRoot)
            {
                if (_indexesByFixedParts.ContainsKey(fixedPartOfName) == false)
                {
                    _indexesByFixedParts.Add(fixedPartOfName, 1);
                }

                int index = _indexesByFixedParts[fixedPartOfName];

                while (true)
                {
                    var newName = $"{fixedPartOfName} {index}";

                    if (_existNames.Add(newName))
                    {
                        _indexesByFixedParts[fixedPartOfName] = index;
                        return newName;
                    }

                    if (index == int.MaxValue)
                    {
                        throw new Exception();
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// Проверить доступность наименования
        /// </summary>
        /// <param name="name">Наименование для проверки</param>
        /// <returns>Результат выполнения операции.</returns>
        public static bool CheckName(string name)
        {
            lock (SyncRoot)
            {
                return _existNames.Contains(name) == false;
            }
        }

        /// <summary>
        /// Добавить занятые наименования
        /// </summary>
        /// <param name="existNames">Наименования</param>
        /// <returns>Результат выполнения операции.</returns>
        public static bool AddExistNames(IEnumerable<string> existNames)
        {
            lock (SyncRoot)
            {
                foreach (string name in existNames)
                {
                    _existNames.Add(name);
                }
            }

            return true;
        }
    }
}
