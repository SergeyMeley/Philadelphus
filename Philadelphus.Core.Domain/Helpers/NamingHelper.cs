namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Помощник наименования
    /// </summary>
    public static class NamingHelper
    {
        /// <summary>
        /// Получить новое наименование
        /// </summary>
        /// <param name="existNames">Коллекция занятых наименований</param>
        /// <param name="fixPart">Фиксированная часть наименования</param>
        /// <returns></returns>
        public static string GetNewName(IEnumerable<string> existNames, string fixPart)
        {
            bool IsIndexExist = true;
            int index = 1;
            string newName = string.Empty;
            do
            {
                newName = $"{fixPart.Trim()} {index}";
                if (existNames == null || existNames.Contains(newName) == false)
                {
                    IsIndexExist = false;
                }
                index++;
            } while (IsIndexExist == true);
            return newName;
        }
    }
}
