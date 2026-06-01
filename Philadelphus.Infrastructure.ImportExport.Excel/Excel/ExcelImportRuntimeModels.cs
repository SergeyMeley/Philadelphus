using ClosedXML.Excel;
using System.Collections.Generic;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    internal sealed class ImportedEntitySet
    {
        public ImportedEntityDefinition Definition { get; set; } = new();

        public IXLRange Range { get; set; } = null!;

        public List<ImportedEntityRow> Rows { get; set; } = new();
    }

    internal sealed class ImportedEntityRow
    {
        public ImportedEntityDefinition Definition { get; set; } = new();

        public int ExcelRowNumber { get; set; }

        public Dictionary<int, string> ValuesByColumnIndex { get; set; } = new();
    }

    internal sealed class ImportGraph
    {
        public List<ImportedEntitySet> EntitySets { get; set; } = new();

        public Dictionary<ImportedEntityRow, List<ImportedEntityRow>> ChildrenByParent { get; set; } = new();

        public HashSet<ImportedEntityRow> ChildRows { get; set; } = new();

        public HashSet<string> RelatedChildSourceNames { get; set; } = new(System.StringComparer.OrdinalIgnoreCase);

        public List<ExcelImportRelationSchema> Relations { get; set; } = new();
    }
}
