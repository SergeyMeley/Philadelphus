using ClosedXML.Excel;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    internal sealed class ExcelImportSourceProjector
    {
        private readonly IExcelImportSourceReader _sourceReader;

        public ExcelImportSourceProjector(IExcelImportSourceReader sourceReader)
        {
            _sourceReader = sourceReader;
        }

        public List<ImportedEntitySet> Project(XLWorkbook workbook, IReadOnlyCollection<ImportedEntityDefinition> definitions)
        {
            var result = new List<ImportedEntitySet>();
            ExcelImportLimits.ValidateSourceCount(definitions.Count);

            foreach (var definition in definitions.Where(x => x.IsEnabled))
            {
                var range = _sourceReader.ResolveRange(workbook, definition.SourceSelection);
                if (range == null)
                    throw new System.InvalidOperationException($"Не удалось найти источник «{definition.SourceName}» в Excel-файле.");
                ExcelImportLimits.ValidateRange(range, definition.SourceName);

                var rows = ExcelImportRangeHelper.GetDataRows(range, definition.DataStartRowOffset)
                    .Select(row => new ImportedEntityRow
                    {
                        Definition = definition,
                        ExcelRowNumber = row.RowNumber(),
                        ValuesByColumnIndex = definition.Properties.ToDictionary(
                            property => property.ColumnIndex,
                            property => ExcelRelationKeyHelper.Normalize(ExcelImportRangeHelper.GetCellText(row, property.ColumnIndex)))
                    })
                    .ToList();

                result.Add(new ImportedEntitySet
                {
                    Definition = definition,
                    Range = range,
                    Rows = rows
                });
            }

            return result;
        }
    }
}
