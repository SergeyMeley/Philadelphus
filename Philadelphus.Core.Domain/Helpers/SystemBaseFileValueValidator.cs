using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Проверяет строковое значение системного типа FILE как сохраняемую ссылку на файловый ресурс.
    /// </summary>
    /// <remarks>
    /// Доменная модель намеренно не проверяет физическую доступность файла через File.Exists: сохраненный файл может быть
    /// временно недоступен, находиться на другой машине или представлять объект внешнего хранилища, например MinIO.
    /// Здесь проверяется только наличие непустого идентификатора ресурса. Проверка доступности должна выполняться
    /// отдельным сервисом/провайдером в сценариях, где файл действительно нужно открыть или прочитать.
    /// </remarks>
    internal static class SystemBaseFileValueValidator
    {
        private static readonly IReadOnlyCollection<Func<string, bool>> _supportedReferenceFormats =
        [
            IsRootedLocalPath,
            IsFileUri,
            IsExternalStorageUri
        ];

        /// <summary>
        /// Проверяет, что значение содержит ссылку на файловый ресурс в одном из поддерживаемых форматов.
        /// </summary>
        /// <param name="value">Проверяемое значение FILE.</param>
        /// <returns>true, если значение является локальным путем, file:// URI или URI внешнего хранилища; иначе false.</returns>
        public static bool IsSupportedReference(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || string.Equals(value, TreeLeaveModel.EmptyStringValue, StringComparison.Ordinal))
            {
                return false;
            }

            return _supportedReferenceFormats.Any(x => x(value));
        }

        private static bool IsRootedLocalPath(string value)
        {
            return Path.IsPathRooted(value)
                && Uri.TryCreate(value, UriKind.Absolute, out var uri) == false;
        }

        private static bool IsFileUri(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.IsFile;
        }

        private static bool IsExternalStorageUri(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.IsFile == false
                && string.IsNullOrWhiteSpace(uri.Scheme) == false;
        }
    }
}
