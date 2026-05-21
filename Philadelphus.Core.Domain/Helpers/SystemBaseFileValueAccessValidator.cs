using System.IO;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Проверяет доступность значения системного типа FILE.
    /// </summary>
    /// <remarks>
    /// Сейчас поддерживаются только файлы локальной файловой системы. Проверка вынесена отдельно,
    /// чтобы позже добавить провайдеры внешних хранилищ, например MinIO, без изменения общего
    /// валидатора системных строковых значений.
    /// </remarks>
    internal static class SystemBaseFileValueAccessValidator
    {
        /// <summary>
        /// Проверяет, указывает ли значение на доступный файл.
        /// </summary>
        /// <param name="value">Строковое значение системного листа FILE.</param>
        /// <returns>true, если файл доступен; иначе false.</returns>
        public static bool IsAvailable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return IsLocalFileAvailable(value);
        }

        /// <summary>
        /// Проверяет доступность локального файла.
        /// </summary>
        /// <remarks>
        /// URI со схемами, отличными от file://, пока считаются неподдержанными. Для MinIO будет добавлена
        /// отдельная ветка проверки, например для minio://bucket/key или другого согласованного формата.
        /// </remarks>
        /// <param name="value">Путь к файлу или file:// URI.</param>
        /// <returns>true, если локальный файл существует; иначе false.</returns>
        private static bool IsLocalFileAvailable(string value)
        {
            try
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    return uri.IsFile && File.Exists(uri.LocalPath);
                }

                return File.Exists(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
