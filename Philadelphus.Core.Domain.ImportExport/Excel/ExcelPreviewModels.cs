using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public enum ExcelPreviewSourceType
    {
        Worksheet,
        NamedRange
    }

    public class ExcelPreviewSourceInfo
    {
        public string Name { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; }

        public int TotalRowCount { get; set; }

        public int TotalColumnCount { get; set; }

        public string StatusText { get; set; } = "Не настроен";
    }

    public class ExcelPreviewWorkbookInfo
    {
        public List<ExcelPreviewSourceInfo> Worksheets { get; set; } = new();

        public List<ExcelPreviewSourceInfo> NamedRanges { get; set; } = new();
    }

    public class ExcelPreviewTable
    {
        public string SourceName { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; }

        public int TotalRowCount { get; set; }

        public int TotalColumnCount { get; set; }

        public List<string> Headers { get; set; } = new();

        public List<List<string>> Rows { get; set; } = new();
    }

    public class ExcelImportSourceSelection
    {
        public string SourceName { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; }
    }

    public enum ExcelImportColumnRole
    {
        Attribute,
        SystemName,
        SystemDescription,
        SystemSequence,
        Ignore
    }

    public class ExcelImportColumnProfile
    {
        public int ColumnIndex { get; set; }

        public string HeaderName { get; set; } = string.Empty;

        public string SampleValue { get; set; } = string.Empty;

        public ExcelImportColumnRole Role { get; set; } = ExcelImportColumnRole.Attribute;

        public string DataTypeNodeName { get; set; } = "Текст";

        public bool IsCollectionValue { get; set; }

        public VisibilityScope Visibility { get; set; } = VisibilityScope.Public;

        public OverrideType Override { get; set; } = OverrideType.Virtual;

        public string VisibilityDisplayName => ExcelImportDisplayHelper.GetDisplayName(Visibility);

        public string OverrideDisplayName => ExcelImportDisplayHelper.GetDisplayName(Override);
    }

    public class ExcelImportProfile
    {
        public ExcelImportSourceSelection SourceSelection { get; set; } = new();

        public List<ExcelImportColumnProfile> Columns { get; set; } = new();
    }

    public class ExcelImportValidationError
    {
        public string SourceName { get; set; } = string.Empty;

        public int RowNumber { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsConfigurationError { get; set; }
    }

    public class ExcelImportValidationResult
    {
        public List<ExcelImportValidationError> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;
    }

    public enum ImportTreePreviewItemType
    {
        Root,
        Node,
        Leaf
    }

    public class ImportTreePreviewModel
    {
        public ImportTreePreviewItem? Root { get; set; }

        public int NodeCount { get; set; }

        public int LeafCount { get; set; }
    }

    public class ImportTreePreviewItem
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DisplayType { get; set; } = string.Empty;

        public ImportTreePreviewItemType ItemType { get; set; }

        public long? Sequence { get; set; }

        public List<ImportTreePreviewAttribute> Attributes { get; set; } = new();

        public List<ImportTreePreviewItem> Childs { get; set; } = new();
    }

    public class ImportTreePreviewAttribute
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DataTypeNodeName { get; set; } = string.Empty;

        public string? ValueLeaveName { get; set; }

        public bool IsCollectionValue { get; set; }

        public VisibilityScope Visibility { get; set; }

        public OverrideType Override { get; set; }

        public string VisibilityDisplayName => ExcelImportDisplayHelper.GetDisplayName(Visibility);

        public string OverrideDisplayName => ExcelImportDisplayHelper.GetDisplayName(Override);
    }

    internal static class ExcelImportDisplayHelper
    {
        internal static string GetDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }
    }
}
