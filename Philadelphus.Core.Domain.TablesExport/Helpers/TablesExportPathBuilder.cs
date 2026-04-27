using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Philadelphus.Core.Domain.TablesExport.Helpers
{
    internal static class TablesExportPathBuilder
    {
        public static string BuildExportPath(string reportName, string extension)
        {
            var now = DateTimeOffset.Now;
            var safeReportName = SanitizeFileName(reportName);

            var timestamp = now
                .ToString("yyyy-MM-dd_HH-mm-ss.fffzzz", CultureInfo.InvariantCulture)
                .Replace(":", "-");

            var fileName =
                $"ph_{safeReportName}_{timestamp}.{extension.TrimStart('.')}";

            var directoryPath = Path.Combine(Path.GetTempPath(), "Philadelphus", "Reports");

            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, fileName);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "report";

            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegex = new Regex($"[{invalidChars}]+");

            return invalidRegex.Replace(value, "_");
        }
    }
}