using ClosedXML.Excel;
using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
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

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ConversionService
    {
        private readonly IExcelDataTypeDetector _dataTypeDetector;

        public ConversionService(IExcelDataTypeDetector dataTypeDetector)
        {
            _dataTypeDetector = dataTypeDetector;
        }

        // Эмуляция получения корней из хранилища "Чубушник"
        public List<string> GetExistingRootsFromStorage()
        {
            // В реальном приложении здесь будет вызов API или БД
            return new List<string> { "Склад", "Сотрудники", "Клиенты", "Архив" };
        }

        public List<TreeRootModel> GetExistingRootsFromStorage(ShrubModel shrub)
        {
            return shrub.ContentWorkingTrees
                .Select(x => x.ContentRoot)
                .Where(x => x != null && x.IsSystemBase == false)
                .ToList();
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput)
        {
            return ProcessFile(filePath, createNewRoot, rootNameInput, (ExcelImportProfile?)null);
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
            string excelFileName = Path.GetFileNameWithoutExtension(filePath);

            // Логика определения имени корня
            string finalRootName = createNewRoot ? rootNameInput : rootNameInput;
            // Примечание: если createNewRoot = false, rootNameInput приходит из ComboBox (выбранный существующий корень)

            var root = new TreeRootExportDTO(finalRootName, $"Импортировано из книги «{excelFileName}»");

            var jsonObject = new WorkingTreeExportDTO(root.Name, root);

            using (var workbook = new XLWorkbook(filePath))
            {
                var allNodes = new List<TreeNodeExportDTO>();

                if (importProfiles == null || importProfiles.Count == 0)
                {
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var worksheetRange = worksheet.RangeUsed();
                        if (worksheetRange == null)
                            continue;

                        allNodes.Add(CreateNodeFromRange(
                            worksheet.Name,
                            $"Импортировано с листа «{worksheet.Name}»",
                            finalRootName,
                            worksheetRange));
                    }
                }
                else
                {
                    foreach (var importProfile in importProfiles)
                    {
                        var selectedRange = ResolveRange(workbook, importProfile.SourceSelection);
                        if (selectedRange == null)
                        {
                            throw new InvalidOperationException($"Не удалось найти лист «{importProfile.SourceSelection.SourceName}» в Excel-файле.");
                        }

                        allNodes.Add(CreateNodeFromRange(
                            importProfile.SourceSelection.SourceName,
                            $"Импортировано с листа «{importProfile.SourceSelection.SourceName}»",
                            finalRootName,
                            selectedRange,
                            importProfile));
                    }
                }

                root.ChildNodes.AddRange(allNodes);
            }

            // Сериализация в JSON
            //var jsonSettings = new JsonSerializerSettings 
            //{ Formatting = Formatting.Indented,  
            //};
            //string jsonResult = JsonConvert.SerializeObject(jsonObject, jsonSettings);


            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,       // A-Z, 0-9, знаки препинания
                UnicodeRanges.Cyrillic),        // А-Я, а-я
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResult = System.Text.Json.JsonSerializer.Serialize(jsonObject, options);


            return jsonResult;
        }

        private TreeNodeExportDTO CreateNodeFromRange(string nodeName, string description, string owningRootName, IXLRange range, ExcelImportProfile? importProfile = null)
        {
            var node = new TreeNodeExportDTO
            {
                Name = nodeName,
                Description = description,
                OwningRootName = owningRootName,
                Attributes = new List<AttributeExportDTO>()
            };

            var firstRow = range.FirstRowUsed();
            if (firstRow == null)
                return node;

            var columnProfiles = BuildColumnProfiles(range, importProfile);

            foreach (var profile in columnProfiles.Where(x => x.Role == ExcelImportColumnRole.Attribute))
            {
                node.Attributes.Add(new AttributeExportDTO(profile.HeaderName, $"Импортировано из колонки «{profile.HeaderName}»")
                {
                    DataTypeNodeName = profile.DataTypeNodeName,
                    ValueLeaveName = null,
                    IsCollectionValue = profile.IsCollectionValue,
                    Visibility = profile.Visibility,
                    Override = profile.Override
                });
            }

            var allRows = range.RowsUsed().ToList();
            var allLeaves = new List<TreeLeaveExportDTO>();
            var usedLeafNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var rowIndex = 1; rowIndex < allRows.Count; rowIndex++)
            {
                var row = allRows[rowIndex];
                var excelRowNumber = row.RowNumber();
                var leafName = ResolveLeafName(columnProfiles, row, excelRowNumber, usedLeafNames);
                var leafDescription = ResolveLeafDescription(columnProfiles, row, excelRowNumber);
                var leaf = new TreeLeaveExportDTO(leafName, leafDescription, nodeName)
                {
                    Sequence = ResolveLeafSequence(columnProfiles, row)
                };

                var attributeProfiles = columnProfiles.Where(x => x.Role == ExcelImportColumnRole.Attribute).ToList();
                for (var attrIndex = 0; attrIndex < attributeProfiles.Count; attrIndex++)
                {
                    var attributeProfile = attributeProfiles[attrIndex];
                    var columnNumber = attributeProfile.ColumnIndex;
                    var cellValue = row.Cell(columnNumber).GetString();
                    var attrDef = node.Attributes[attrIndex];

                    leaf.Attributes.Add(new AttributeExportDTO(attrDef.Name, attrDef.Description)
                    {
                        DataTypeNodeName = attrDef.DataTypeNodeName,
                        ValueLeaveName = cellValue,
                        IsCollectionValue = attrDef.IsCollectionValue,
                        Visibility = attrDef.Visibility,
                        Override = attrDef.Override
                    });
                }

                allLeaves.Add(leaf);
            }

            node.ChildLeaves.AddRange(allLeaves);
            return node;
        }

        private List<ExcelImportColumnProfile> BuildColumnProfiles(IXLRange range, ExcelImportProfile? importProfile)
        {
            if (importProfile?.Columns?.Count > 0)
                return importProfile.Columns.OrderBy(x => x.ColumnIndex).ToList();

            var firstRow = range.FirstRowUsed();
            var firstColumn = range.FirstColumnUsed();
            var lastColumn = range.LastColumnUsed();

            if (firstRow == null || firstColumn == null || lastColumn == null)
                return new List<ExcelImportColumnProfile>();

            var headerRowNumber = firstRow.RowNumber();
            var firstColumnNumber = firstColumn.ColumnNumber();
            var lastColumnNumber = lastColumn.ColumnNumber();
            var result = new List<ExcelImportColumnProfile>();

            for (var absoluteColumnNumber = firstColumnNumber; absoluteColumnNumber <= lastColumnNumber; absoluteColumnNumber++)
            {
                var relativeColumnIndex = absoluteColumnNumber - firstColumnNumber + 1;
                var headerValue = range.Worksheet.Cell(headerRowNumber, absoluteColumnNumber).GetFormattedString();

                result.Add(new ExcelImportColumnProfile
                {
                    ColumnIndex = relativeColumnIndex,
                    HeaderName = string.IsNullOrWhiteSpace(headerValue) ? $"Колонка {relativeColumnIndex}" : headerValue,
                    Role = ExcelImportColumnRole.Attribute,
                    DataTypeNodeName = DetermineDataType(range, relativeColumnIndex)
                });
            }

            return result;
        }

        private static string ResolveLeafName(List<ExcelImportColumnProfile> columnProfiles, IXLRangeRow row, int excelRowNumber, HashSet<string> usedLeafNames)
        {
            var nameColumn = columnProfiles.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemName);
            var baseName = nameColumn == null
                ? excelRowNumber.ToString()
                : row.Cell(nameColumn.ColumnIndex).GetString().Trim();

            if (string.IsNullOrWhiteSpace(baseName))
                baseName = excelRowNumber.ToString();

            var uniqueName = baseName;
            var counter = 2;

            while (usedLeafNames.Contains(uniqueName))
            {
                uniqueName = $"{baseName} ({counter})";
                counter++;
            }

            usedLeafNames.Add(uniqueName);
            return uniqueName;
        }

        private static string ResolveLeafDescription(List<ExcelImportColumnProfile> columnProfiles, IXLRangeRow row, int excelRowNumber)
        {
            var descriptionColumn = columnProfiles.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemDescription);
            var description = descriptionColumn == null
                ? string.Empty
                : row.Cell(descriptionColumn.ColumnIndex).GetString().Trim();

            return string.IsNullOrWhiteSpace(description)
                ? $"Импортировано из строки «{excelRowNumber}»"
                : description;
        }

        private static long? ResolveLeafSequence(List<ExcelImportColumnProfile> columnProfiles, IXLRangeRow row)
        {
            var sequenceColumn = columnProfiles.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemSequence);
            if (sequenceColumn == null)
                return null;

            var sequenceValue = row.Cell(sequenceColumn.ColumnIndex).GetString().Trim();
            return long.TryParse(sequenceValue, out var parsed) ? parsed : null;
        }

        private IXLRange? ResolveRange(XLWorkbook workbook, ExcelImportSourceSelection sourceSelection)
        {
            if (sourceSelection.SourceType == ExcelPreviewSourceType.Worksheet)
            {
                var worksheet = workbook.Worksheets.FirstOrDefault(x => string.Equals(x.Name, sourceSelection.SourceName, StringComparison.OrdinalIgnoreCase));
                return worksheet?.RangeUsed();
            }

            var namedRange = workbook.DefinedNames
                .Concat(workbook.Worksheets.SelectMany(x => x.DefinedNames))
                .FirstOrDefault(x => string.Equals(x.Name, sourceSelection.SourceName, StringComparison.OrdinalIgnoreCase));

            return namedRange?.Ranges.FirstOrDefault()?.RangeAddress.AsRange();
        }

        private string DetermineDataType(IXLRange range, int colNumber)
        {
            var values = range.RowsUsed()
                .Skip(1)
                .Select(row => row.Cell(colNumber).GetString())
                .ToList();

            return _dataTypeDetector.DetermineBestDataType(values);
        }
    }
}
