using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public static class ExcelImportSchemaNormalizer
    {
        public static void EnsureEditableState(ExcelImportSchema schema)
        {
            ArgumentNullException.ThrowIfNull(schema);

            schema.Sheets ??= new List<ExcelImportSheetSchema>();
            schema.Relations ??= new List<ExcelImportRelationSchema>();
            schema.Entities ??= new List<ImportedEntityDefinition>();

            foreach (var sheet in schema.Sheets)
            {
                EnsureSheetState(sheet);
            }
        }

        public static void ApplyRelationProjectionToSheets(ExcelImportSchema schema)
        {
            EnsureEditableState(schema);

            foreach (var relation in schema.Relations.Where(x => x.IsEnabled))
            {
                var childSheet = schema.Sheets.FirstOrDefault(x =>
                    string.Equals(x.SourceName, relation.ChildSourceName, StringComparison.OrdinalIgnoreCase));

                if (childSheet == null)
                    continue;

                childSheet.Profile.Relation.ParentSourceName = relation.ParentSourceName;
                childSheet.Profile.Relation.ParentKeyColumnName = relation.ParentKeyColumnName;
                childSheet.Profile.Relation.ChildKeyColumnName = relation.ChildKeyColumnName;
            }
        }

        public static void NormalizeForExecution(ExcelImportSchema schema)
        {
            EnsureEditableState(schema);

            foreach (var sheet in schema.Sheets)
            {
                NormalizeSheetForExecution(sheet);
            }

            RefreshRelationProjection(schema);
            schema.Entities = BuildEntityDefinitions(schema).ToList();
        }

        public static List<ExcelImportProfile> GetEnabledProfiles(ExcelImportSchema schema)
        {
            NormalizeForExecution(schema);

            return schema.Sheets
                .Where(x => x.IsEnabled)
                .Select(x => x.Profile)
                .ToList();
        }

        public static ExcelImportSchema GetCanonicalExecutionSchema(ExcelImportSchema schema)
        {
            NormalizeForExecution(schema);
            return schema;
        }

        public static void RefreshRelationProjection(ExcelImportSchema schema)
        {
            EnsureEditableState(schema);
            schema.Relations = BuildRelationProjection(schema).ToList();
        }

        public static IReadOnlyList<ExcelImportRelationSchema> BuildRelationProjection(ExcelImportSchema schema)
        {
            EnsureEditableState(schema);

            return schema.Sheets
                .Where(x => x.Profile.Relation.HasParent)
                .Select(x => new ExcelImportRelationSchema
                {
                    ParentSourceName = x.Profile.Relation.ParentSourceName,
                    ChildSourceName = x.SourceName,
                    ParentKeyColumnName = x.Profile.Relation.ParentKeyColumnName,
                    ChildKeyColumnName = x.Profile.Relation.ChildKeyColumnName,
                    IsEnabled = x.IsEnabled
                })
                .ToList();
        }

        private static void NormalizeSheetForExecution(ExcelImportSheetSchema sheet)
        {
            EnsureSheetState(sheet);
            ExcelImportProfileEditorHelper.SyncSheetFieldsFromProfile(sheet);

            ExcelImportProfileEditorHelper.ApplyAdditionalSystemRole(
                sheet.Profile.Columns,
                sheet.RowNameColumnName,
                ExcelImportColumnRole.SystemName);

            ExcelImportProfileEditorHelper.ApplySingleSystemRole(
                sheet.Profile.Columns,
                sheet.DescriptionColumnName,
                ExcelImportColumnRole.SystemDescription);

            ExcelImportProfileEditorHelper.ApplySingleSystemRole(
                sheet.Profile.Columns,
                sheet.SequenceColumnName,
                ExcelImportColumnRole.SystemSequence);

            sheet.Profile.SourceSelection.SourceName = sheet.SourceName;
            sheet.Profile.SourceSelection.SourceType = sheet.SourceType;
            sheet.Profile.EntityKind = sheet.EntityKind;
        }

        private static IReadOnlyList<ImportedEntityDefinition> BuildEntityDefinitions(ExcelImportSchema schema)
        {
            return schema.Sheets
                .Where(x => x.IsEnabled)
                .Select(sheet => new ImportedEntityDefinition
                {
                    EntityId = sheet.SourceName,
                    SourceName = sheet.SourceName,
                    SourceType = sheet.SourceType,
                    DisplayName = string.IsNullOrWhiteSpace(sheet.DisplayName) ? sheet.SourceName : sheet.DisplayName,
                    IsEnabled = sheet.IsEnabled,
                    EntityKind = sheet.EntityKind,
                    KeyColumnName = sheet.RowKeyColumnName,
                    NameColumnName = sheet.RowNameColumnName,
                    DescriptionColumnName = sheet.DescriptionColumnName,
                    SequenceColumnName = sheet.SequenceColumnName,
                    DataStartRowOffset = ExcelImportRangeHelper.NormalizeDataStartRowOffset(sheet.Profile.DataStartRowOffset),
                    Properties = sheet.Profile.Columns
                        .OrderBy(column => column.ColumnIndex)
                        .Select(column => new PropertyDefinition
                        {
                            ColumnIndex = column.ColumnIndex,
                            SourceColumnName = column.HeaderName,
                            PropertyName = column.HeaderName,
                            Role = column.Role,
                            Placement = column.Placement,
                            PropagationMode = column.PropagationMode,
                            DefaultValue = column.DefaultValue,
                            Description = column.Description,
                            DataTypeNodeName = column.DataTypeNodeName,
                            IsCollectionValue = column.IsCollectionValue,
                            Visibility = column.Visibility,
                            Override = column.Override
                        })
                        .ToList()
                })
                .ToList();
        }

        private static void EnsureSheetState(ExcelImportSheetSchema sheet)
        {
            sheet.Profile ??= new ExcelImportProfile();
            sheet.Profile.SourceSelection ??= new ExcelImportSourceSelection();
            sheet.Profile.Relation ??= new ExcelImportRelationProfile();
            sheet.Profile.Columns ??= new List<ExcelImportColumnProfile>();

            if (string.IsNullOrWhiteSpace(sheet.DisplayName))
            {
                sheet.DisplayName = sheet.SourceName;
            }

            sheet.Profile.SourceSelection.SourceName = sheet.SourceName;
            sheet.Profile.SourceSelection.SourceType = sheet.SourceType;
        }
    }
}
