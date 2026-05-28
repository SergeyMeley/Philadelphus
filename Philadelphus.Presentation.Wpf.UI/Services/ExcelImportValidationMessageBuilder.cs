using Philadelphus.Infrastructure.ImportExport.Excel;
using System;
using System.Linq;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    internal static class ExcelImportValidationMessageBuilder
    {
        internal static string Build(ExcelImportValidationResult validationResult)
        {
            var lines = validationResult.Errors
                .Take(15)
                .Select(error =>
                {
                    if (error.IsConfigurationError)
                    {
                        return string.IsNullOrWhiteSpace(error.ColumnName)
                            ? $"Лист \"{error.SourceName}\": {error.Message}"
                            : $"Лист \"{error.SourceName}\", настройка колонки \"{error.ColumnName}\": {error.Message}";
                    }

                    return $"Лист \"{error.SourceName}\", строка {error.RowNumber}, колонка \"{error.ColumnName}\": {error.Message}";
                })
                .ToList();

            if (validationResult.Errors.Count > 15)
            {
                lines.Add($"... и еще {validationResult.Errors.Count - 15} ошибок.");
            }

            lines.Insert(0, "Операция остановлена. В настройках или данных импорта найдены ошибки.");
            lines.Insert(1, string.Empty);

            return string.Join(Environment.NewLine, lines);
        }
    }
}
