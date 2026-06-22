using System;
using System.IO;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Кроссплатформенное раскрытие путей из конфигурации.
    /// </summary>
    /// <remarks>
    /// На Linux/macOS <see cref="Environment.ExpandEnvironmentVariables"/> не раскрывает синтаксис
    /// <c>%VAR%</c> и не знает <c>USERPROFILE</c>/<c>LOCALAPPDATA</c>/<c>APPDATA</c>, а обратные слэши из
    /// Windows-конфига там не являются разделителями пути. Хелпер приводит такие пути к рабочему виду на
    /// любой ОС; на Windows поведение совпадает с прежним <c>ExpandEnvironmentVariables</c>.
    /// </remarks>
    public static class ConfigPathHelper
    {
        /// <summary>
        /// Раскрывает путь из конфигурации: известные Windows-переменные среды → соответствующие каталоги,
        /// прочие <c>%VAR%</c> — через <see cref="Environment.ExpandEnvironmentVariables"/>, обратные слэши →
        /// нативный разделитель (на не-Windows).
        /// </summary>
        /// <param name="path">Исходный путь из конфигурации (может быть null).</param>
        /// <returns>Раскрытый путь.</returns>
        public static string Expand(string? path)
        {
            var result = (path ?? string.Empty)
                .Replace("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                .Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                .Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            result = Environment.ExpandEnvironmentVariables(result);

            if (Path.DirectorySeparatorChar != '\\')
            {
                result = result.Replace('\\', Path.DirectorySeparatorChar);
            }

            return result;
        }
    }
}
