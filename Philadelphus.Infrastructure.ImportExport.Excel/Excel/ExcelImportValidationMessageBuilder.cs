namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Формирует текстовое описание ошибок проверки Excel-импорта.
    /// </summary>
    public static class ExcelImportValidationMessageBuilder
    {
        /// <summary>
        /// Формирует сообщение об ошибках проверки.
        /// </summary>
        /// <param name="validationResult">Результат проверки Excel-импорта.</param>
        /// <returns>Текстовое описание ошибок.</returns>
        public static string Build(ExcelImportValidationResult validationResult)
        {
            ArgumentNullException.ThrowIfNull(validationResult);

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
