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
        NamedRange,
        Table
    }

    public class ExcelPreviewSourceInfo
    {
        public string Name { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; }

        public string WorksheetName { get; set; } = string.Empty;

        public int TotalRowCount { get; set; }

        public int TotalColumnCount { get; set; }

        public string StatusText { get; set; } = "Не настроен";
    }

    public class ExcelPreviewWorkbookInfo
    {
        public List<ExcelPreviewSourceInfo> Worksheets { get; set; } = new();

        public List<ExcelPreviewSourceInfo> NamedRanges { get; set; } = new();

        public List<ExcelPreviewSourceInfo> Tables { get; set; } = new();
    }

    public class ExcelPreviewTable
    {
        public string SourceName { get; set; } = string.Empty;

        public ExcelPreviewSourceType SourceType { get; set; }

        public string WorksheetName { get; set; } = string.Empty;

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

    public enum ExcelImportPropertyPlacement
    {
        [Display(Name = "Корень")]
        Root,

        [Display(Name = "Узел")]
        Node,

        [Display(Name = "Лист")]
        Leaf
    }

    public enum ExcelImportValuePropagationMode
    {
        [Display(Name = "Без распространения")]
        None,

        [Display(Name = "Константа на родителе")]
        ParentConstant,

        [Display(Name = "От родителя, если пусто")]
        ParentFallbackIfEmpty
    }

    public enum ExcelImportEntityKind
    {
        [Display(Name = "Корень")]
        Root,
        
        [Display(Name = "Узлы")]
        Node,

        [Display(Name = "Листья")]
        Leaf
    }

    public class ExcelImportColumnProfile
    {
        public int ColumnIndex { get; set; }

        public string HeaderName { get; set; } = string.Empty;

        public string SampleValue { get; set; } = string.Empty;

        public ExcelImportColumnRole Role { get; set; } = ExcelImportColumnRole.Attribute;

        public string Description { get; set; } = string.Empty;

        public string DataTypeNodeName { get; set; } = "Текст";

        public bool IsCollectionValue { get; set; }

        public VisibilityScope Visibility { get; set; } = VisibilityScope.Public;

        public OverrideType Override { get; set; } = OverrideType.Virtual;

        public ExcelImportPropertyPlacement Placement { get; set; } = ExcelImportPropertyPlacement.Leaf;

        public ExcelImportValuePropagationMode PropagationMode { get; set; } = ExcelImportValuePropagationMode.None;

        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Сколько первых used rows источника пропускаем перед чтением значений этой колонки.
        /// 1 = только header row; 2 = header row + строка маркеров.
        /// </summary>
        public int DataStartRowOffset { get; set; } = 1;

        public string PlacementDisplayName => ExcelImportDisplayHelper.GetDisplayName(Placement);

        public string PropagationModeDisplayName => ExcelImportDisplayHelper.GetDisplayName(PropagationMode);

        // Совместимость со старыми экранами/шаблонами: canonical pipeline использует Placement.
        public ExcelImportDefinitionScope DefinitionScope
        {
            get => Placement == ExcelImportPropertyPlacement.Root
                ? ExcelImportDefinitionScope.Root
                : ExcelImportDefinitionScope.Node;
            set => Placement = value == ExcelImportDefinitionScope.Root
                ? ExcelImportPropertyPlacement.Root
                : ExcelImportPropertyPlacement.Leaf;
        }

        // Совместимость со старыми экранами/шаблонами: canonical pipeline использует PropagationMode.
        public ExcelImportValueMode ValueMode
        {
            get => PropagationMode switch
            {
                ExcelImportValuePropagationMode.ParentConstant => ExcelImportValueMode.InheritedConstant,
                ExcelImportValuePropagationMode.ParentFallbackIfEmpty => ExcelImportValueMode.InheritedIfEmpty,
                _ => ExcelImportValueMode.PerLeaf
            };
            set => PropagationMode = value switch
            {
                ExcelImportValueMode.InheritedConstant => ExcelImportValuePropagationMode.ParentConstant,
                ExcelImportValueMode.InheritedIfEmpty => ExcelImportValuePropagationMode.ParentFallbackIfEmpty,
                _ => ExcelImportValuePropagationMode.None
            };
        }

        public string DefinitionScopeDisplayName => PlacementDisplayName;

        public string ValueModeDisplayName => PropagationModeDisplayName;

        public string VisibilityDisplayName => ExcelImportDisplayHelper.GetDisplayName(Visibility);

        public string OverrideDisplayName => ExcelImportDisplayHelper.GetDisplayName(Override);
    }

    public class ExcelImportProfile
    {
        public ExcelImportSourceSelection SourceSelection { get; set; } = new();

        public ExcelImportRelationProfile Relation { get; set; } = new();

        public ExcelImportEntityKind EntityKind { get; set; } = ExcelImportEntityKind.Leaf;

        public List<ExcelImportColumnProfile> Columns { get; set; } = new();

        /// <summary>
        /// Каноническое смещение строк данных для всего листа/диапазона.
        /// Дублируется в колонках при выполнении, потому что часть resolver-ов получает только column profile.
        /// </summary>
        public int DataStartRowOffset { get; set; } = 1;
    }

    public class ExcelImportRelationProfile
    {
        public string ParentSourceName { get; set; } = string.Empty;

        public string ParentKeyColumnName { get; set; } = string.Empty;

        public string ChildKeyColumnName { get; set; } = string.Empty;

        public bool HasParent => string.IsNullOrWhiteSpace(ParentSourceName) == false;
    }

    public enum ExcelImportDefinitionScope
    {
        [Display(Name = "Узел")]
        Node,

        [Display(Name = "Корень")]
        Root
    }

    public enum ExcelImportValueMode
    {
        [Display(Name = "Значение строки")]
        PerLeaf,

        [Display(Name = "Константа на родителе")]
        InheritedConstant,

        [Display(Name = "От родителя, если пусто")]
        InheritedIfEmpty
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

    public class ExcelImportDefinitionScopeItem
    {
        public ExcelImportPropertyPlacement Value { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    public class ExcelImportValueModeItem
    {
        public ExcelImportValuePropagationMode Value { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    public class ExcelImportEntityKindItem
    {
        public ExcelImportEntityKind Value { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    public class ExcelImportSettingsRowDto
    {
        public string RuleType { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string ParentSourceName { get; set; } = string.Empty;

        public string ParentKeyColumnName { get; set; } = string.Empty;

        public string ChildKeyColumnName { get; set; } = string.Empty;

        public int? ColumnIndex { get; set; }

        public string HeaderName { get; set; } = string.Empty;

        public ExcelImportColumnRole? Role { get; set; }

        public ExcelImportDefinitionScope? DefinitionScope { get; set; }

        public ExcelImportValueMode? ValueMode { get; set; }

        public ExcelImportPropertyPlacement? Placement { get; set; }

        public ExcelImportValuePropagationMode? PropagationMode { get; set; }

        public ExcelImportEntityKind? EntityKind { get; set; }

        public string DataTypeNodeName { get; set; } = string.Empty;

        public bool? IsCollectionValue { get; set; }

        public VisibilityScope? Visibility { get; set; }

        public OverrideType? Override { get; set; }

        public string Description { get; set; } = string.Empty;

        public string DefaultValue { get; set; } = string.Empty;
    }

    public class ExcelImportSettingsDocument
    {
        public List<ExcelImportSettingsRowDto> WorkbookDefaults { get; set; } = new();

        public List<ExcelImportSettingsRowDto> WorksheetDefaults { get; set; } = new();

        public List<ExcelImportSettingsRowDto> ColumnRules { get; set; } = new();
    }

    public class ExcelImportInheritanceInfo
    {
        public IReadOnlyList<string> DistinctNonEmptyValues { get; set; } = Array.Empty<string>();

        public string? ResolvedParentValue { get; set; }

        public bool HasResolvedParentValue => string.IsNullOrWhiteSpace(ResolvedParentValue) == false;
    }

    public static class ExcelImportDisplayHelper
    {
        public static string GetDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }
    }
}
