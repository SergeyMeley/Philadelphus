using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelPreviewService
    {
        private readonly IExcelDataTypeDetector _dataTypeDetector;
        private readonly IExcelImportSourceReader _sourceReader;
        private readonly IExcelImportProfileResolver _profileResolver;
        private readonly IExcelImportProfileValidator _profileValidator;

        public ExcelPreviewService(
            IExcelDataTypeDetector dataTypeDetector,
            IExcelImportSourceReader sourceReader,
            IExcelImportProfileResolver profileResolver,
            IExcelImportProfileValidator profileValidator)
        {
            _dataTypeDetector = dataTypeDetector;
            _sourceReader = sourceReader;
            _profileResolver = profileResolver;
            _profileValidator = profileValidator;
        }

        public ExcelPreviewWorkbookInfo GetWorkbookPreview(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);

            var result = new ExcelPreviewWorkbookInfo
            {
                Worksheets = _sourceReader.GetImportableWorksheets(workbook)
                    .Select(worksheet => CreateSourceInfo(worksheet.Name, ExcelPreviewSourceType.Worksheet, worksheet.Name, worksheet.RangeUsed()))
                    .ToList(),
                Tables = _sourceReader.GetImportableTables(workbook)
                    .Select(table => CreateSourceInfo(table.Name, ExcelPreviewSourceType.Table, table.Worksheet.Name, table))
                    .ToList(),
                NamedRanges = _sourceReader.GetImportableNamedRanges(workbook)
                    .Select(namedRange =>
                    {
                        var range = GetSingleRange(namedRange);
                        return CreateSourceInfo(namedRange.Name, ExcelPreviewSourceType.NamedRange, range?.Worksheet.Name ?? string.Empty, range);
                    })
                    .ToList()
            };

            return result;
        }

        public ExcelPreviewTable GetPreview(string filePath, ExcelImportSourceSelection selection, int maxRows = 30)
        {
            using var workbook = new XLWorkbook(filePath);
            var range = _sourceReader.ResolveRange(workbook, selection);

            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для предпросмотра.");

            return BuildPreviewTable(selection.SourceName, selection.SourceType, range, maxRows);
        }

        public ExcelImportProfile BuildImportProfile(string filePath, ExcelImportSourceSelection selection)
        {
            using var workbook = new XLWorkbook(filePath);
            var range = _sourceReader.ResolveRange(workbook, selection);

            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для построения профиля импорта.");

            var preview = BuildPreviewTable(selection.SourceName, selection.SourceType, range, maxRows: 5);
            var profile = new ExcelImportProfile
            {
                SourceSelection = selection,
                Columns = new List<ExcelImportColumnProfile>()
            };

            var rows = range.RowsUsed().ToList();
            // Есть два поддерживаемых формата:
            // 1) маркеры прямо в первой строке: #Name | #Описание | #Sequence;
            // 2) обычные заголовки в первой строке и маркеры во второй.
            var markerRoles = ExcelImportRangeHelper.DetectMarkerRoles(rows, preview.Headers.Count);
            var dataStartRowOffset = markerRoles.Count > 0 ? 2 : 1;
            profile.DataStartRowOffset = dataStartRowOffset;
            var dataRows = rows
                .Skip(dataStartRowOffset)
                .ToList();

            for (var i = 0; i < preview.Headers.Count; i++)
            {
                var columnIndex = i + 1;
                var rawHeader = rows.Count == 0
                    ? string.Empty
                    : ExcelImportRangeHelper.GetCellText(rows[0], columnIndex);
                var header = preview.Headers[i];
                var sampleValue = dataRows.FirstOrDefault() == null
                    ? string.Empty
                    : ExcelImportRangeHelper.GetCellText(dataRows[0], columnIndex);
                var role = markerRoles.TryGetValue(columnIndex, out var markerRole)
                    ? markerRole
                    : ExcelImportColumnRoleHelper.DetectRole(rawHeader);
                var allValues = dataRows
                    .Select(row => ExcelImportRangeHelper.GetCellText(row, columnIndex))
                    .ToList();
                // Пустые разделительные колонки в подготовленном Excel не должны создавать атрибут "Колонка N".
                if (string.IsNullOrWhiteSpace(rawHeader) && allValues.All(x => string.IsNullOrWhiteSpace(x)))
                {
                    role = ExcelImportColumnRole.Ignore;
                }

                profile.Columns.Add(new ExcelImportColumnProfile
                {
                    ColumnIndex = columnIndex,
                    HeaderName = header,
                    SampleValue = sampleValue,
                    Role = role,
                    Description = $"Импортировано из колонки «{header}»",
                    DataTypeNodeName = DetermineDataTypeFromValues(allValues),
                    DataStartRowOffset = dataStartRowOffset
                });
            }

            return _profileResolver.Resolve(filePath, selection, profile);
        }

        public ExcelImportValidationResult ValidateImportProfile(string filePath, ExcelImportProfile profile)
        {
            return _profileValidator.ValidateProfile(filePath, profile);
        }

        public ExcelImportValidationResult ValidateProfileConfiguration(ExcelImportProfile profile)
        {
            return _profileValidator.ValidateConfiguration(profile);
        }

        private static ExcelPreviewSourceInfo CreateSourceInfo(string name, ExcelPreviewSourceType sourceType, string worksheetName, IXLRange? range)
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
                WorksheetName = worksheetName,
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
                    SourceType = sourceType,
                    WorksheetName = range.Worksheet.Name
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
                WorksheetName = range.Worksheet.Name,
                TotalRowCount = lastRowNumber - headerRowNumber,
                TotalColumnCount = headers.Count,
                Headers = headers,
                Rows = rows
            };
        }

        private static IXLRange? GetSingleRange(IXLDefinedName definedName)
        {
            var ranges = definedName.Ranges.ToList();

            if (ranges.Count == 0)
                return null;

            return ranges[0].RangeAddress.AsRange();
        }

        private string DetermineDataTypeFromValues(IEnumerable<string?> values)
        {
            return _dataTypeDetector.DetermineBestDataType(values);
        }

    }
}
