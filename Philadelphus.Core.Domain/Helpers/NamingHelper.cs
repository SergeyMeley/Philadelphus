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
        private static HashSet<string> _existNames = new HashSet<string>();
        private static Dictionary<string, int> _indexesByFixedParts = new Dictionary<string, int>();

        /// <summary>
        /// Использованные (занятые) наименования
        /// </summary>
        public static ReadOnlySet<string> ExistNames { get => _existNames.AsReadOnly(); }

        /// <summary>
        /// Получить новое наименование
        /// </summary>
        /// <param name="fixedPartOfName">Фиксированная часть наименования</param>
        /// <param name="existNames">Коллекция занятых наименований</param>
        /// <returns>Результат выполнения операции.</returns>
        public static string GetNewName(string fixedPartOfName = "Новое наименование")
        {
            if (_indexesByFixedParts.ContainsKey(fixedPartOfName) == false)
            {
                _indexesByFixedParts.Add(fixedPartOfName, 1);
            }

            int index = _indexesByFixedParts[fixedPartOfName];
            string newName = string.Empty;

            while (true)
            {
                newName = $"{fixedPartOfName} {index}";

                if (CheckName(newName))
                {
                    _existNames.Add(newName);
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

        /// <summary>
        /// Проверить доступность наименования
        /// </summary>
        /// <param name="name">Наименование для проверки</param>
        /// <returns>Результат выполнения операции.</returns>
        public static bool CheckName(string name)
        {
            return ExistNames.Any(x => x == name) == false;
        }

        /// <summary>
        /// Добавить занятые наименования
        /// </summary>
        /// <param name="existNames">Наименования</param>
        /// <returns>Результат выполнения операции.</returns>
        public static bool AddExistNames(IEnumerable<string> existNames)
        {
            foreach (string name in existNames?.Where(x => ExistNames.Contains(x) == false))
            {
                _existNames.Add(name);
            }
            return true;
        }
    }
}
