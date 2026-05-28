using System.Collections.Generic;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportSchema
    {
        public string Name { get; set; } = string.Empty;

        public string SourceFilePath { get; set; } = string.Empty;

        public string RootName { get; set; } = string.Empty;

        public bool CreateNewRoot { get; set; } = true;

        public List<ImportedEntityDefinition> Entities { get; set; } = new();

        public List<ExcelImportSheetSchema> Sheets { get; set; } = new();

        public List<ExcelImportRelationSchema> Relations { get; set; } = new();
    }

    public class ExcelImportSheetSchema
    {
        public string SourceName { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; } = ExcelPreviewSourceType.Worksheet;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public ExcelImportEntityKind EntityKind { get; set; } = ExcelImportEntityKind.Leaf;

        public string EntityKindDisplayName => ExcelImportDisplayHelper.GetDisplayName(EntityKind);

        public string RowKeyColumnName { get; set; } = string.Empty;

        public string RowNameColumnName { get; set; } = string.Empty;

        public string DescriptionColumnName { get; set; } = string.Empty;

        public string SequenceColumnName { get; set; } = string.Empty;

        public double CanvasX { get; set; }

        public double CanvasY { get; set; }

        public ExcelImportProfile Profile { get; set; } = new();
    }

    public class ExcelImportRelationSchema
    {
        public string ParentSourceName { get; set; } = string.Empty;

        public string ChildSourceName { get; set; } = string.Empty;

        public string ParentKeyColumnName { get; set; } = string.Empty;

        public string ChildKeyColumnName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
    }

    public class ImportedEntityDefinition
    {
        public string EntityId { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; } = ExcelPreviewSourceType.Worksheet;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public ExcelImportEntityKind EntityKind { get; set; } = ExcelImportEntityKind.Leaf;

        public string KeyColumnName { get; set; } = string.Empty;

        public string NameColumnName { get; set; } = string.Empty;

        public string DescriptionColumnName { get; set; } = string.Empty;

        public string SequenceColumnName { get; set; } = string.Empty;

        public int DataStartRowOffset { get; set; } = 1;

        public List<PropertyDefinition> Properties { get; set; } = new();

        public ExcelImportSourceSelection SourceSelection => new()
        {
            SourceName = SourceName,
            SourceType = SourceType
        };
    }

    public class PropertyDefinition
    {
        public int ColumnIndex { get; set; }

        public string SourceColumnName { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public ExcelImportColumnRole Role { get; set; } = ExcelImportColumnRole.Attribute;

        public ExcelImportPropertyPlacement Placement { get; set; } = ExcelImportPropertyPlacement.Leaf;

        public ExcelImportValuePropagationMode PropagationMode { get; set; } = ExcelImportValuePropagationMode.None;

        public string DefaultValue { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DataTypeNodeName { get; set; } = "Текст";

        public bool IsCollectionValue { get; set; }

        public Philadelphus.Core.Domain.Entities.Enums.VisibilityScope Visibility { get; set; }
            = Philadelphus.Core.Domain.Entities.Enums.VisibilityScope.Public;

        public Philadelphus.Core.Domain.Entities.Enums.OverrideType Override { get; set; }
            = Philadelphus.Core.Domain.Entities.Enums.OverrideType.Virtual;
    }

    public class RelationDefinition
    {
        public string ParentEntityId { get; set; } = string.Empty;

        public string ChildEntityId { get; set; } = string.Empty;

        public string ParentKeyColumnName { get; set; } = string.Empty;

        public string ChildKeyColumnName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
    }
}
