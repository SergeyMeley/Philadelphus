using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelPreviewService
    {
        private readonly IExcelDataTypeDetector _dataTypeDetector;

        public ExcelPreviewService(IExcelDataTypeDetector dataTypeDetector)
        {
            _dataTypeDetector = dataTypeDetector;
        }

        public ExcelPreviewWorkbookInfo GetWorkbookPreview(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);

            var result = new ExcelPreviewWorkbookInfo
            {
                Worksheets = workbook.Worksheets
                    .Select(worksheet => CreateSourceInfo(worksheet.Name, ExcelPreviewSourceType.Worksheet, worksheet.RangeUsed()))
                    .ToList(),
                NamedRanges = GetNamedRanges(workbook)
                    .Select(namedRange => CreateSourceInfo(namedRange.Name, ExcelPreviewSourceType.NamedRange, GetSingleRange(namedRange)))
                    .ToList()
            };

            return result;
        }

        public ExcelPreviewTable GetPreview(string filePath, ExcelImportSourceSelection selection, int maxRows = 30)
        {
            using var workbook = new XLWorkbook(filePath);
            var range = ResolveRange(workbook, selection);

            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для предпросмотра.");

            return BuildPreviewTable(selection.SourceName, selection.SourceType, range, maxRows);
        }

        public ExcelImportProfile BuildImportProfile(string filePath, ExcelImportSourceSelection selection)
        {
            using var workbook = new XLWorkbook(filePath);
            var range = ResolveRange(workbook, selection);

            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для построения профиля импорта.");

            var preview = BuildPreviewTable(selection.SourceName, selection.SourceType, range, maxRows: 5);
            var profile = new ExcelImportProfile
            {
                SourceSelection = selection,
                Columns = new List<ExcelImportColumnProfile>()
            };

            var rows = range.RowsUsed().ToList();
            for (var i = 0; i < preview.Headers.Count; i++)
            {
                var header = preview.Headers[i];
                var sampleValue = preview.Rows.FirstOrDefault()?.ElementAtOrDefault(i) ?? string.Empty;
                var allValues = rows
                    .Skip(1)
                    .Select(row => row.Cell(i + 1).GetString())
                    .ToList();

                profile.Columns.Add(new ExcelImportColumnProfile
                {
                    ColumnIndex = i + 1,
                    HeaderName = header,
                    SampleValue = sampleValue,
                    Role = DetectRole(header),
                    DataTypeNodeName = DetermineDataTypeFromValues(allValues)
                });
            }

            return profile;
        }

        public ExcelImportValidationResult ValidateImportProfile(string filePath, ExcelImportProfile profile)
        {
            var configurationValidation = ValidateProfileConfiguration(profile);
            if (configurationValidation.HasErrors)
                return configurationValidation;

            using var workbook = new XLWorkbook(filePath);
            var range = ResolveRange(workbook, profile.SourceSelection);

            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для проверки импорта.");

            var result = new ExcelImportValidationResult();
            var rows = range.RowsUsed().ToList();

            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];

                foreach (var column in profile.Columns.Where(x => x.Role == ExcelImportColumnRole.Attribute))
                {
                    var value = row.Cell(column.ColumnIndex).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    if (IsValueCompatibleWithDataType(value, column.DataTypeNodeName))
                        continue;

                    result.Errors.Add(new ExcelImportValidationError
                    {
                        SourceName = profile.SourceSelection.SourceName,
                        RowNumber = row.RowNumber(),
                        ColumnName = column.HeaderName,
                        Value = value,
                        Message = $"Значение \"{value}\" не соответствует типу \"{column.DataTypeNodeName}\"."
                    });
                }

                var sequenceColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemSequence);
                if (sequenceColumn != null)
                {
                    var sequenceValue = row.Cell(sequenceColumn.ColumnIndex).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(sequenceValue) == false && long.TryParse(sequenceValue, out _) == false)
                    {
                        result.Errors.Add(new ExcelImportValidationError
                        {
                            SourceName = profile.SourceSelection.SourceName,
                            RowNumber = row.RowNumber(),
                            ColumnName = sequenceColumn.HeaderName,
                            Value = sequenceValue,
                            Message = $"Значение \"{sequenceValue}\" не соответствует типу \"Целое число\" для последовательности."
                        });
                    }
                }
            }

            return result;
        }

        public ExcelImportValidationResult ValidateProfileConfiguration(ExcelImportProfile profile)
        {
            var result = new ExcelImportValidationResult();
            var columns = profile.Columns;

            var systemNameColumns = columns.Where(x => x.Role == ExcelImportColumnRole.SystemName).ToList();
            if (systemNameColumns.Count > 1)
            {
                foreach (var column in systemNameColumns.Skip(1))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Назначено больше одной колонки с ролью SystemName."));
                }
            }

            var systemDescriptionColumns = columns.Where(x => x.Role == ExcelImportColumnRole.SystemDescription).ToList();
            if (systemDescriptionColumns.Count > 1)
            {
                foreach (var column in systemDescriptionColumns.Skip(1))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Назначено больше одной колонки с ролью SystemDescription."));
                }
            }

            var systemSequenceColumns = columns.Where(x => x.Role == ExcelImportColumnRole.SystemSequence).ToList();
            if (systemSequenceColumns.Count > 1)
            {
                foreach (var column in systemSequenceColumns.Skip(1))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Назначено больше одной колонки с ролью SystemSequence."));
                }
            }

            if (columns.Any(x => x.Role == ExcelImportColumnRole.Attribute) == false)
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    string.Empty,
                    "Не выбрано ни одной колонки с ролью Attribute."));
            }

            return result;
        }

        private static ExcelPreviewSourceInfo CreateSourceInfo(string name, ExcelPreviewSourceType sourceType, IXLRange? range)
        {
            var firstRow = range?.FirstRowUsed();
            var lastRow = range?.LastRowUsed();
            var firstColumn = range?.FirstColumnUsed();
            var lastColumn = range?.LastColumnUsed();

            var totalRows = firstRow == null || lastRow == null
                ? 0
                : lastRow.RowNumber() - firstRow.RowNumber() + 1;

            var totalColumns = firstColumn == null || lastColumn == null
                ? 0
                : lastColumn.ColumnNumber() - firstColumn.ColumnNumber() + 1;

            return new ExcelPreviewSourceInfo
            {
                Name = name,
                SourceType = sourceType,
                TotalRowCount = totalRows,
                TotalColumnCount = totalColumns
            };
        }

        private static ExcelPreviewTable BuildPreviewTable(string sourceName, ExcelPreviewSourceType sourceType, IXLRange range, int maxRows)
        {
            var firstRow = range.FirstRowUsed();
            var lastRow = range.LastRowUsed();
            var firstColumn = range.FirstColumnUsed();
            var lastColumn = range.LastColumnUsed();

            if (firstRow == null || lastRow == null || firstColumn == null || lastColumn == null)
            {
                return new ExcelPreviewTable
                {
                    SourceName = sourceName,
                    SourceType = sourceType
                };
            }

            var headerRowNumber = firstRow.RowNumber();
            var firstColumnNumber = firstColumn.ColumnNumber();
            var lastColumnNumber = lastColumn.ColumnNumber();
            var lastRowNumber = lastRow.RowNumber();

            var headers = new List<string>();
            for (var columnNumber = firstColumnNumber; columnNumber <= lastColumnNumber; columnNumber++)
            {
                var headerValue = range.Worksheet.Cell(headerRowNumber, columnNumber).GetFormattedString();
                headers.Add(string.IsNullOrWhiteSpace(headerValue) ? $"Колонка {columnNumber - firstColumnNumber + 1}" : headerValue);
            }

            var rows = new List<List<string>>();
            var previewLastRowNumber = Math.Min(lastRowNumber, headerRowNumber + maxRows);

            for (var rowNumber = headerRowNumber + 1; rowNumber <= previewLastRowNumber; rowNumber++)
            {
                var rowValues = new List<string>();
                for (var columnNumber = firstColumnNumber; columnNumber <= lastColumnNumber; columnNumber++)
                {
                    rowValues.Add(range.Worksheet.Cell(rowNumber, columnNumber).GetFormattedString());
                }

                rows.Add(rowValues);
            }

            return new ExcelPreviewTable
            {
                SourceName = sourceName,
                SourceType = sourceType,
                TotalRowCount = lastRowNumber - headerRowNumber,
                TotalColumnCount = headers.Count,
                Headers = headers,
                Rows = rows
            };
        }

        private static List<IXLDefinedName> GetNamedRanges(XLWorkbook workbook)
        {
            var workbookRanges = workbook.DefinedNames.ToList();
            var worksheetRanges = workbook.Worksheets
                .SelectMany(worksheet => worksheet.DefinedNames)
                .ToList();

            return workbookRanges
                .Concat(worksheetRanges)
                .Where(range => range.IsValid)
                .GroupBy(range => range.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static IXLRange? ResolveRange(XLWorkbook workbook, ExcelImportSourceSelection selection)
        {
            if (selection.SourceType == ExcelPreviewSourceType.Worksheet)
            {
                var worksheet = workbook.Worksheets.FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));
                return worksheet?.RangeUsed();
            }

            var namedRange = GetNamedRanges(workbook)
                .FirstOrDefault(x => string.Equals(x.Name, selection.SourceName, StringComparison.OrdinalIgnoreCase));

            return namedRange == null ? null : GetSingleRange(namedRange);
        }

        private static IXLRange? GetSingleRange(IXLDefinedName definedName)
        {
            var ranges = definedName.Ranges.ToList();

            if (ranges.Count == 0)
                return null;

            return ranges[0].RangeAddress.AsRange();
        }

        private static ExcelImportColumnRole DetectRole(string header)
        {
            var normalized = (header ?? string.Empty).Trim().ToLowerInvariant();

            return normalized switch
            {
                "#name" => ExcelImportColumnRole.SystemName,
                "#описание" => ExcelImportColumnRole.SystemDescription,
                "#description" => ExcelImportColumnRole.SystemDescription,
                "#sequence" => ExcelImportColumnRole.SystemSequence,
                _ when normalized.StartsWith("#") => ExcelImportColumnRole.Ignore,
                _ => ExcelImportColumnRole.Attribute
            };
        }

        private string DetermineDataTypeFromValues(List<string?> values)
        {
            return _dataTypeDetector.DetermineBestDataType(values);
        }

        private bool IsValueCompatibleWithDataType(string value, string dataTypeNodeName)
        {
            return _dataTypeDetector.IsValueCompatibleWithDataType(value, dataTypeNodeName);
        }

        private static ExcelImportValidationError CreateConfigurationError(string sourceName, string columnName, string message)
        {
            return new ExcelImportValidationError
            {
                SourceName = sourceName,
                ColumnName = columnName,
                Message = message,
                IsConfigurationError = true
            };
        }
    }
}
