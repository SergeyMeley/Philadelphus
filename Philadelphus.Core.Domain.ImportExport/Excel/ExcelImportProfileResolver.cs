using System;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportProfileResolver : IExcelImportProfileResolver
    {
        private readonly IExcelImportSettingsReader _settingsReader;

        public ExcelImportProfileResolver(IExcelImportSettingsReader settingsReader)
        {
            _settingsReader = settingsReader;
        }

        public ExcelImportProfile Resolve(string filePath, ExcelImportSourceSelection selection, ExcelImportProfile detectedProfile)
        {
            var settings = _settingsReader.Read(filePath);
            var resolvedProfile = CloneProfile(detectedProfile);

            var workbookDefault = settings.WorkbookDefaults.FirstOrDefault();
            var worksheetDefault = settings.WorksheetDefaults
                .FirstOrDefault(x => string.Equals(x.SourceName, selection.SourceName, StringComparison.OrdinalIgnoreCase));

            ApplyRelation(resolvedProfile.Relation, workbookDefault);
            ApplyRelation(resolvedProfile.Relation, worksheetDefault);

            foreach (var column in resolvedProfile.Columns)
            {
                ApplyRow(column, workbookDefault);
                ApplyRow(column, worksheetDefault);

                var columnRule = settings.ColumnRules.FirstOrDefault(rule =>
                    string.Equals(rule.SourceName, selection.SourceName, StringComparison.OrdinalIgnoreCase)
                    && (rule.ColumnIndex == column.ColumnIndex
                        || (rule.ColumnIndex == null
                            && string.Equals(rule.HeaderName, column.HeaderName, StringComparison.OrdinalIgnoreCase))));

                ApplyRow(column, columnRule);
            }

            return resolvedProfile;
        }

        private static ExcelImportProfile CloneProfile(ExcelImportProfile source)
        {
            return new ExcelImportProfile
            {
                SourceSelection = new ExcelImportSourceSelection
                {
                    SourceName = source.SourceSelection.SourceName,
                    SourceType = source.SourceSelection.SourceType
                },
                Columns = source.Columns.Select(column => new ExcelImportColumnProfile
                {
                    ColumnIndex = column.ColumnIndex,
                    HeaderName = column.HeaderName,
                    SampleValue = column.SampleValue,
                    Role = column.Role,
                    Description = column.Description,
                    DataTypeNodeName = column.DataTypeNodeName,
                    IsCollectionValue = column.IsCollectionValue,
                    Visibility = column.Visibility,
                    Override = column.Override,
                    DefinitionScope = column.DefinitionScope,
                    ValueMode = column.ValueMode,
                    DefaultValue = column.DefaultValue
                }).ToList(),
                Relation = new ExcelImportRelationProfile
                {
                    ParentSourceName = source.Relation.ParentSourceName,
                    ParentKeyColumnName = source.Relation.ParentKeyColumnName,
                    ChildKeyColumnName = source.Relation.ChildKeyColumnName
                }
            };
        }

        private static void ApplyRelation(ExcelImportRelationProfile relation, ExcelImportSettingsRowDto? settingsRow)
        {
            if (settingsRow == null)
                return;

            if (string.IsNullOrWhiteSpace(settingsRow.ParentSourceName) == false)
                relation.ParentSourceName = settingsRow.ParentSourceName;

            if (string.IsNullOrWhiteSpace(settingsRow.ParentKeyColumnName) == false)
                relation.ParentKeyColumnName = settingsRow.ParentKeyColumnName;

            if (string.IsNullOrWhiteSpace(settingsRow.ChildKeyColumnName) == false)
                relation.ChildKeyColumnName = settingsRow.ChildKeyColumnName;
        }

        private static void ApplyRow(ExcelImportColumnProfile column, ExcelImportSettingsRowDto? settingsRow)
        {
            if (settingsRow == null)
                return;

            if (settingsRow.Role.HasValue)
                column.Role = settingsRow.Role.Value;

            if (string.IsNullOrWhiteSpace(settingsRow.Description) == false)
                column.Description = settingsRow.Description;

            if (string.IsNullOrWhiteSpace(settingsRow.DataTypeNodeName) == false)
                column.DataTypeNodeName = settingsRow.DataTypeNodeName;

            if (settingsRow.IsCollectionValue.HasValue)
                column.IsCollectionValue = settingsRow.IsCollectionValue.Value;

            if (settingsRow.Visibility.HasValue)
                column.Visibility = settingsRow.Visibility.Value;

            if (settingsRow.Override.HasValue)
                column.Override = settingsRow.Override.Value;

            if (settingsRow.DefinitionScope.HasValue)
                column.DefinitionScope = settingsRow.DefinitionScope.Value;

            if (settingsRow.ValueMode.HasValue)
                column.ValueMode = settingsRow.ValueMode.Value;

            if (string.IsNullOrWhiteSpace(settingsRow.DefaultValue) == false)
                column.DefaultValue = settingsRow.DefaultValue;
        }
    }
}
