using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportSchemaBuilder : IExcelImportSchemaBuilder
    {
        private readonly ExcelPreviewService _previewService;
        private readonly IExcelImportSourceReader _sourceReader;

        public ExcelImportSchemaBuilder(
            ExcelPreviewService previewService,
            IExcelImportSourceReader sourceReader)
        {
            _previewService = previewService;
            _sourceReader = sourceReader;
        }

        public ExcelImportSchema CreateDraftSchema(string filePath, string rootName)
        {
            var workbookPreview = _previewService.GetWorkbookPreview(filePath);
            var schema = new ExcelImportSchema
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                SourceFilePath = filePath,
                RootName = rootName
            };

            using var workbook = new XLWorkbook(filePath);
            var sourceSelections = _sourceReader.GetDefaultSourceSelections(workbook);
            var previewSources = workbookPreview.Tables
                .Concat(workbookPreview.NamedRanges)
                .Concat(workbookPreview.Worksheets)
                .ToList();
            var sheets = new List<ExcelImportSheetSchema>();
            for (var index = 0; index < sourceSelections.Count; index++)
            {
                var selection = sourceSelections[index];
                var source = previewSources.FirstOrDefault(x =>
                    x.SourceType == selection.SourceType
                    && string.Equals(x.Name, selection.SourceName, System.StringComparison.OrdinalIgnoreCase));
                var profile = _previewService.BuildImportProfile(filePath, selection);
                var rowNameColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemName)?.HeaderName ?? string.Empty;
                var rowDescriptionColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemDescription)?.HeaderName ?? string.Empty;
                var rowSequenceColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemSequence)?.HeaderName ?? string.Empty;
                var rowKeyColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.Attribute)?.HeaderName ?? string.Empty;

                sheets.Add(new ExcelImportSheetSchema
                {
                    SourceName = selection.SourceName,
                    SourceType = selection.SourceType,
                    DisplayName = source?.Name ?? selection.SourceName,
                    EntityKind = profile.EntityKind,
                    RowKeyColumnName = rowKeyColumn,
                    RowNameColumnName = rowNameColumn,
                    DescriptionColumnName = rowDescriptionColumn,
                    SequenceColumnName = rowSequenceColumn,
                    CanvasX = 40 + (index % 3) * 280,
                    CanvasY = 40 + (index / 3) * 220,
                    Profile = profile
                });
            }

            schema.Sheets = sheets;
            ExcelImportSchemaNormalizer.RefreshRelationProjection(schema);

            return schema;
        }
    }
}
