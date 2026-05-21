using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public static class ExcelImportProfileEditorHelper
    {
        public static void ApplySingleSystemRole(IEnumerable<ExcelImportColumnProfile> columns, string? headerName, ExcelImportColumnRole role)
        {
            foreach (var column in columns.Where(x => x.Role == role))
            {
                column.Role = ExcelImportColumnRole.Attribute;
            }

            if (string.IsNullOrWhiteSpace(headerName))
                return;

            var targetColumn = columns.FirstOrDefault(x => string.Equals(x.HeaderName, headerName, StringComparison.OrdinalIgnoreCase));
            if (targetColumn != null)
            {
                targetColumn.Role = role;
            }
        }

        public static void ApplyAdditionalSystemRole(IEnumerable<ExcelImportColumnProfile> columns, string? headerName, ExcelImportColumnRole role)
        {
            if (string.IsNullOrWhiteSpace(headerName))
                return;

            var targetColumn = columns.FirstOrDefault(x => string.Equals(x.HeaderName, headerName, StringComparison.OrdinalIgnoreCase));
            if (targetColumn != null)
            {
                targetColumn.Role = role;
            }
        }

        public static void SyncSheetFieldsFromProfile(ExcelImportSheetSchema sheet)
        {
            if (string.IsNullOrWhiteSpace(sheet.DisplayName))
            {
                sheet.DisplayName = sheet.SourceName;
            }

            sheet.RowNameColumnName = sheet.Profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemName)?.HeaderName ?? string.Empty;
            sheet.DescriptionColumnName = sheet.Profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemDescription)?.HeaderName ?? string.Empty;
            sheet.SequenceColumnName = sheet.Profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemSequence)?.HeaderName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sheet.RowKeyColumnName))
            {
                sheet.RowKeyColumnName = sheet.Profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.Attribute)?.HeaderName ?? string.Empty;
            }
        }

        public static List<string> BuildHeaderOptions(IEnumerable<ExcelImportColumnProfile> columns, bool sort = true)
        {
            var headers = columns.Select(x => x.HeaderName);
            return sort
                ? headers.OrderBy(x => x).ToList()
                : headers.ToList();
        }

        public static List<string> BuildParentSourceOptions(IEnumerable<string> sourceNames, string currentSourceName, string noParentOption)
        {
            var result = new List<string> { noParentOption };
            result.AddRange(sourceNames
                .Where(x => string.Equals(x, currentSourceName, StringComparison.OrdinalIgnoreCase) == false)
                .OrderBy(x => x));
            return result;
        }

        public static string GetSelectedParentOption(string? parentSourceName, IEnumerable<string> availableOptions, string noParentOption)
        {
            if (string.IsNullOrWhiteSpace(parentSourceName))
                return noParentOption;

            var selectedParent = parentSourceName;
            return availableOptions.Contains(selectedParent, StringComparer.OrdinalIgnoreCase)
                ? selectedParent
                : noParentOption;
        }

        public static bool TrySetRelationParent(
            IEnumerable<ExcelImportSheetSchema> sheets,
            ExcelImportSheetSchema childSheet,
            string? parentSourceName,
            string noParentOption,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            var sheetList = sheets.ToList();

            if (string.IsNullOrWhiteSpace(parentSourceName) || string.Equals(parentSourceName, noParentOption, StringComparison.OrdinalIgnoreCase))
            {
                ClearRelation(childSheet);
                return true;
            }

            if (TryResolveRelationParent(sheetList, childSheet, parentSourceName, out var parentSheet, out errorMessage) == false)
                return false;

            var parentChanged = string.Equals(
                childSheet.Profile.Relation.ParentSourceName,
                parentSheet.SourceName,
                StringComparison.OrdinalIgnoreCase) == false;
            childSheet.Profile.Relation.ParentSourceName = parentSheet.SourceName;
            if (parentChanged || ContainsHeader(parentSheet.Profile.Columns, childSheet.Profile.Relation.ParentKeyColumnName) == false)
            {
                childSheet.Profile.Relation.ParentKeyColumnName = string.Empty;
            }

            if (ContainsHeader(childSheet.Profile.Columns, childSheet.Profile.Relation.ChildKeyColumnName) == false)
            {
                childSheet.Profile.Relation.ChildKeyColumnName = string.Empty;
            }

            return true;
        }

        public static bool TrySetRelationColumns(
            IEnumerable<ExcelImportSheetSchema> sheets,
            ExcelImportSheetSchema childSheet,
            string? parentSourceName,
            string? parentKeyColumnName,
            string? childKeyColumnName,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            var sheetList = sheets.ToList();

            if (string.IsNullOrWhiteSpace(parentSourceName))
            {
                errorMessage = "Не выбран родительский лист.";
                return false;
            }

            if (TryResolveRelationParent(sheetList, childSheet, parentSourceName, out var parentSheet, out errorMessage) == false)
                return false;

            if (string.IsNullOrWhiteSpace(parentKeyColumnName))
            {
                errorMessage = "Не выбрана колонка родителя для связи.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(childKeyColumnName))
            {
                errorMessage = "Не выбрана колонка дочернего листа для связи.";
                return false;
            }

            if (ContainsHeader(parentSheet.Profile.Columns, parentKeyColumnName) == false)
            {
                errorMessage = $"В родительском листе \"{parentSheet.SourceName}\" не найдена колонка \"{parentKeyColumnName}\".";
                return false;
            }

            if (ContainsHeader(childSheet.Profile.Columns, childKeyColumnName) == false)
            {
                errorMessage = $"В дочернем листе \"{childSheet.SourceName}\" не найдена колонка \"{childKeyColumnName}\".";
                return false;
            }

            childSheet.Profile.Relation.ParentSourceName = parentSheet.SourceName;
            childSheet.Profile.Relation.ParentKeyColumnName = parentKeyColumnName;
            childSheet.Profile.Relation.ChildKeyColumnName = childKeyColumnName;

            return true;
        }

        public static void SetRelationParentKey(ExcelImportSheetSchema childSheet, string? parentKeyColumnName)
        {
            childSheet.Profile.Relation.ParentKeyColumnName = parentKeyColumnName ?? string.Empty;
        }

        public static void SetRelationChildKey(ExcelImportSheetSchema childSheet, string? childKeyColumnName)
        {
            childSheet.Profile.Relation.ChildKeyColumnName = childKeyColumnName ?? string.Empty;
        }

        public static void ClearRelation(ExcelImportSheetSchema childSheet)
        {
            childSheet.Profile.Relation.ParentSourceName = string.Empty;
            childSheet.Profile.Relation.ParentKeyColumnName = string.Empty;
            childSheet.Profile.Relation.ChildKeyColumnName = string.Empty;
        }

        private static bool TryResolveRelationParent(
            IReadOnlyCollection<ExcelImportSheetSchema> sheets,
            ExcelImportSheetSchema childSheet,
            string parentSourceName,
            out ExcelImportSheetSchema parentSheet,
            out string errorMessage)
        {
            parentSheet = sheets.FirstOrDefault(x => string.Equals(x.SourceName, parentSourceName, StringComparison.OrdinalIgnoreCase))!;
            if (parentSheet == null)
            {
                errorMessage = $"Не найден родительский лист \"{parentSourceName}\".";
                return false;
            }

            if (ReferenceEquals(childSheet, parentSheet))
            {
                errorMessage = "Лист не может быть родителем сам для себя.";
                return false;
            }

            if (WouldCreateRelationCycle(sheets, childSheet, parentSheet))
            {
                errorMessage = "Такая связь создаст цикл между листами.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool ContainsHeader(IEnumerable<ExcelImportColumnProfile> columns, string? headerName)
        {
            return string.IsNullOrWhiteSpace(headerName) == false
                && columns.Any(x => string.Equals(x.HeaderName, headerName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool WouldCreateRelationCycle(
            IReadOnlyCollection<ExcelImportSheetSchema> sheets,
            ExcelImportSheetSchema childSheet,
            ExcelImportSheetSchema parentSheet)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cursor = parentSheet;
            while (string.IsNullOrWhiteSpace(cursor.Profile.Relation.ParentSourceName) == false)
            {
                if (visited.Add(cursor.SourceName) == false)
                    return true;

                if (string.Equals(cursor.Profile.Relation.ParentSourceName, childSheet.SourceName, StringComparison.OrdinalIgnoreCase))
                    return true;

                var next = sheets.FirstOrDefault(x => string.Equals(x.SourceName, cursor.Profile.Relation.ParentSourceName, StringComparison.OrdinalIgnoreCase));
                if (next == null)
                    return false;

                cursor = next;
            }

            return false;
        }
    }
}
