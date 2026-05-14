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
                    Description = $"Импортировано из колонки «{header}»",
                    DataTypeNodeName = DetermineDataTypeFromValues(allValues)
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

    }
}
