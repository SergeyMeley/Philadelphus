using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public static class ExcelImportDisplayOptionsHelper
    {
        public static List<VisibilityScopeItem> CreateVisibilityScopeItems()
        {
            return Enum.GetValues<VisibilityScope>()
                .Select(x => new VisibilityScopeItem
                {
                    Value = x,
                    DisplayName = ExcelImportDisplayHelper.GetDisplayName(x)
                })
                .ToList();
        }

        public static List<OverrideTypeItem> CreateOverrideTypeItems()
        {
            return Enum.GetValues<OverrideType>()
                .Select(x => new OverrideTypeItem
                {
                    Value = x,
                    DisplayName = ExcelImportDisplayHelper.GetDisplayName(x)
                })
                .ToList();
        }

        public static List<ExcelImportDefinitionScopeItem> CreateDefinitionScopeItems()
        {
            return Enum.GetValues<ExcelImportPropertyPlacement>()
                .Select(x => new ExcelImportDefinitionScopeItem
                {
                    Value = x,
                    DisplayName = ExcelImportDisplayHelper.GetDisplayName(x)
                })
                .ToList();
        }

        public static List<ExcelImportValueModeItem> CreateValueModeItems()
        {
            return Enum.GetValues<ExcelImportValuePropagationMode>()
                .Select(x => new ExcelImportValueModeItem
                {
                    Value = x,
                    DisplayName = ExcelImportDisplayHelper.GetDisplayName(x)
                })
                .ToList();
        }

        public static List<ExcelImportEntityKindItem> CreateEntityKindItems()
        {
            return Enum.GetValues<ExcelImportEntityKind>()
                .Select(x => new ExcelImportEntityKindItem
                {
                    Value = x,
                    DisplayName = ExcelImportDisplayHelper.GetDisplayName(x)
                })
                .ToList();
        }
    }
}
