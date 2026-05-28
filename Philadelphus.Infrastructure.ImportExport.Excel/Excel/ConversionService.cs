using ClosedXML.Excel;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public class ConversionService
    {
        private readonly IExcelDataTypeDetector _dataTypeDetector;
        private readonly IExcelImportSourceReader _sourceReader;

        public ConversionService(
            IExcelDataTypeDetector dataTypeDetector,
            IExcelImportSourceReader sourceReader,
            IExcelImportInheritanceResolver inheritanceResolver)
        {
            _dataTypeDetector = dataTypeDetector;
            _sourceReader = sourceReader;
        }

        public List<TreeRootModel> GetExistingRootsFromStorage(ShrubModel shrub)
        {
            // В UI нельзя показывать системное дерево базовых типов.
            return shrub.ContentWorkingTrees
                .Where(x => x.IsSystemBase == false)
                .Select(x => x.ContentRoot)
                .Where(x => x != null && x.IsSystemBase == false)
                .ToList();
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput)
        {
            return ProcessFile(filePath, createNewRoot, rootNameInput, (IReadOnlyCollection<ExcelImportProfile>?)null);
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput, ExcelImportSourceSelection? sourceSelection)
        {
            return ProcessFile(
                filePath,
                createNewRoot,
                rootNameInput,
                sourceSelection == null
                    ? null
                    : new List<ExcelImportProfile>
                    {
                        new()
                        {
                            SourceSelection = sourceSelection
                        }
                    });
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput, ExcelImportProfile? importProfile)
        {
            return ProcessFile(
                filePath,
                createNewRoot,
                rootNameInput,
                importProfile == null ? null : new List<ExcelImportProfile> { importProfile });
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput, IReadOnlyCollection<ExcelImportProfile>? importProfiles)
        {
            using var workbook = new XLWorkbook(filePath);
            var schema = BuildSchemaFromProfiles(filePath, createNewRoot, rootNameInput, workbook, importProfiles);
            return ProcessSchema(workbook, schema);
        }

        public string ProcessSchema(ExcelImportSchema schema)
        {
            using var workbook = new XLWorkbook(schema.SourceFilePath);
            return ProcessSchema(workbook, ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema));
        }

        private string ProcessSchema(XLWorkbook workbook, ExcelImportSchema schema)
        {
            ExcelImportSchemaNormalizer.NormalizeForExecution(schema);

            var projector = new ExcelImportSourceProjector(_sourceReader);
            var graphBuilder = new ExcelImportRelationGraphBuilder();
            var propertyResolver = new ExcelImportPropertyResolver();
            var materializer = new ExcelImportMaterializer(propertyResolver);

            var entitySets = projector.Project(workbook, schema.Entities);
            var graph = graphBuilder.Build(entitySets, schema.Relations);
            var payload = materializer.Materialize(schema, graph);

            return JsonSerializer.Serialize(payload, CreateJsonOptions());
        }

        private ExcelImportSchema BuildSchemaFromProfiles(
            string filePath,
            bool createNewRoot,
            string rootNameInput,
            XLWorkbook workbook,
            IReadOnlyCollection<ExcelImportProfile>? importProfiles)
        {
            var schema = new ExcelImportSchema
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                SourceFilePath = filePath,
                RootName = rootNameInput,
                CreateNewRoot = createNewRoot
            };

            var profiles = importProfiles?.Count > 0
                ? importProfiles.ToList()
                : BuildDefaultProfiles(workbook);

            foreach (var profile in profiles)
            {
                var range = _sourceReader.ResolveRange(workbook, profile.SourceSelection);
                if (range == null)
                    throw new InvalidOperationException($"Не удалось найти источник «{profile.SourceSelection.SourceName}» в Excel-файле.");

                var columns = BuildColumnProfiles(range, profile);
                var entity = new ImportedEntityDefinition
                {
                    EntityId = profile.SourceSelection.SourceName,
                    SourceName = profile.SourceSelection.SourceName,
                    SourceType = profile.SourceSelection.SourceType,
                    DisplayName = profile.SourceSelection.SourceName,
                    EntityKind = profile.EntityKind,
                    DataStartRowOffset = ExcelImportRangeHelper.NormalizeDataStartRowOffset(profile.DataStartRowOffset),
                    Properties = columns
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
                };

                schema.Entities.Add(entity);
                schema.Sheets.Add(new ExcelImportSheetSchema
                {
                    SourceName = entity.SourceName,
                    SourceType = entity.SourceType,
                    DisplayName = entity.DisplayName,
                    EntityKind = entity.EntityKind,
                    Profile = profile
                });

                if (profile.Relation.HasParent)
                {
                    schema.Relations.Add(new ExcelImportRelationSchema
                    {
                        ParentSourceName = profile.Relation.ParentSourceName,
                        ChildSourceName = profile.SourceSelection.SourceName,
                        ParentKeyColumnName = profile.Relation.ParentKeyColumnName,
                        ChildKeyColumnName = profile.Relation.ChildKeyColumnName,
                        IsEnabled = true
                    });
                }
            }

            return schema;
        }

        private List<ExcelImportProfile> BuildDefaultProfiles(XLWorkbook workbook)
        {
            return _sourceReader.GetDefaultSourceSelections(workbook)
                .Select(source => new ExcelImportProfile
                {
                    SourceSelection = source,
                    EntityKind = ExcelImportEntityKind.Leaf
                })
                .ToList();
        }

        private List<ExcelImportColumnProfile> BuildColumnProfiles(IXLRange range, ExcelImportProfile? importProfile)
        {
            if (importProfile?.Columns?.Count > 0)
            {
                var offset = ExcelImportRangeHelper.NormalizeDataStartRowOffset(importProfile.DataStartRowOffset);
                foreach (var column in importProfile.Columns)
                    column.DataStartRowOffset = offset;

                return importProfile.Columns.OrderBy(x => x.ColumnIndex).ToList();
            }

            var firstRow = range.FirstRowUsed();
            var firstColumn = range.FirstColumnUsed();
            var lastColumn = range.LastColumnUsed();
            if (firstRow == null || firstColumn == null || lastColumn == null)
                return new List<ExcelImportColumnProfile>();

            var headerRowNumber = firstRow.RowNumber();
            var firstColumnNumber = firstColumn.ColumnNumber();
            var lastColumnNumber = lastColumn.ColumnNumber();
            var columnCount = lastColumnNumber - firstColumnNumber + 1;
            var rows = range.RowsUsed().ToList();
            var markerRoles = ExcelImportRangeHelper.DetectMarkerRoles(rows, columnCount);
            var dataStartRowOffset = markerRoles.Count > 0 ? 2 : 1;
            var result = new List<ExcelImportColumnProfile>();

            for (var absoluteColumnNumber = firstColumnNumber; absoluteColumnNumber <= lastColumnNumber; absoluteColumnNumber++)
            {
                var relativeColumnIndex = absoluteColumnNumber - firstColumnNumber + 1;
                var headerValue = range.Worksheet.Cell(headerRowNumber, absoluteColumnNumber).GetFormattedString();
                var role = markerRoles.TryGetValue(relativeColumnIndex, out var markerRole)
                    ? markerRole
                    : ExcelImportColumnRoleHelper.DetectRole(headerValue);
                var columnValues = ExcelImportRangeHelper.GetDataRows(range, dataStartRowOffset)
                    .Select(row => ExcelImportRangeHelper.GetCellText(row, relativeColumnIndex))
                    .ToList();

                if (string.IsNullOrWhiteSpace(headerValue) && columnValues.All(x => string.IsNullOrWhiteSpace(x)))
                    role = ExcelImportColumnRole.Ignore;

                result.Add(new ExcelImportColumnProfile
                {
                    ColumnIndex = relativeColumnIndex,
                    HeaderName = string.IsNullOrWhiteSpace(headerValue) ? $"Колонка {relativeColumnIndex}" : headerValue,
                    Role = role,
                    DataTypeNodeName = _dataTypeDetector.DetermineBestDataType(columnValues),
                    DataStartRowOffset = dataStartRowOffset
                });
            }

            return result;
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
