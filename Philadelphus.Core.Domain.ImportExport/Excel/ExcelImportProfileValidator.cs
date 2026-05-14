using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportProfileValidator : IExcelImportProfileValidator
    {
        //TODO: Список должен быть из основного сервиса а не самописный
        private static readonly HashSet<string> SupportedDataTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Текст",
            "Число",
            "Целое число",
            "Дробное число"
        };

        private readonly IExcelImportSourceReader _sourceReader;
        private readonly IExcelDataTypeDetector _dataTypeDetector;
        private readonly IExcelImportInheritanceResolver _inheritanceResolver;

        public ExcelImportProfileValidator(
            IExcelImportSourceReader sourceReader,
            IExcelDataTypeDetector dataTypeDetector,
            IExcelImportInheritanceResolver inheritanceResolver)
        {
            _sourceReader = sourceReader;
            _dataTypeDetector = dataTypeDetector;
            _inheritanceResolver = inheritanceResolver;
        }

        public ExcelImportValidationResult ValidateProfile(string filePath, ExcelImportProfile profile)
        {
            var result = ValidateConfiguration(profile);
            if (result.HasErrors)
                return result;

            using var workbook = new XLWorkbook(filePath);
            var range = _sourceReader.ResolveRange(workbook, profile.SourceSelection);
            if (range == null)
                throw new InvalidOperationException("Не удалось определить диапазон данных для проверки импорта.");

            foreach (var column in profile.Columns.Where(x => x.Role == ExcelImportColumnRole.Attribute))
            {
                var inheritanceInfo = _inheritanceResolver.Resolve(range, column);
                ValidateAttributeColumn(profile, range, column, inheritanceInfo, result);
            }

            ValidateSequenceColumn(profile, range, result);
            return result;
        }

        public ExcelImportValidationResult ValidateConfiguration(ExcelImportProfile profile)
        {
            return ValidateConfigurationInternal(profile);
        }

        public ExcelImportValidationResult ValidateProfiles(string filePath, IEnumerable<ExcelImportProfile> profiles)
        {
            var result = new ExcelImportValidationResult();
            var profileList = profiles.ToList();

            ValidateRelations(profileList, result);

            if (result.HasErrors == false)
            {
                ValidateRelationMatches(filePath, profileList, result);
            }

            foreach (var profile in profileList)
            {
                var profileResult = ValidateProfile(filePath, profile);
                result.Errors.AddRange(profileResult.Errors);
            }

            ValidateRootScopeConflicts(filePath, profileList, result);
            return result;
        }

        private static ExcelImportValidationResult ValidateConfigurationInternal(ExcelImportProfile profile)
        {
            var result = new ExcelImportValidationResult();
            var columns = profile.Columns;

            ValidateSingleSystemColumn(profile, columns, ExcelImportColumnRole.SystemName, result);
            ValidateSingleSystemColumn(profile, columns, ExcelImportColumnRole.SystemDescription, result);
            ValidateSingleSystemColumn(profile, columns, ExcelImportColumnRole.SystemSequence, result);

            if (columns.Any(x => x.Role == ExcelImportColumnRole.Attribute) == false)
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    string.Empty,
                    "Не выбрано ни одной колонки с ролью Attribute."));
            }

            if (profile.Relation.HasParent)
            {
                if (string.IsNullOrWhiteSpace(profile.Relation.ParentKeyColumnName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Для связи с родителем не указана колонка ключа у родителя."));
                }

                if (string.IsNullOrWhiteSpace(profile.Relation.ChildKeyColumnName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Для связи с родителем не указана колонка ключа у текущего листа."));
                }
            }

            foreach (var column in columns.Where(x => x.Role == ExcelImportColumnRole.Attribute))
            {
                if (SupportedDataTypes.Contains(column.DataTypeNodeName) == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        $"Тип данных \"{column.DataTypeNodeName}\" не поддерживается импортом из Excel."));
                }

                if (column.DefinitionScope == ExcelImportDefinitionScope.Root
                    && column.ValueMode != ExcelImportValueMode.InheritedConstant)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Для DefinitionScope=Root поддерживается только режим InheritedConstant."));
                }

                if (column.ValueMode == ExcelImportValueMode.InheritedIfEmpty
                    && string.IsNullOrWhiteSpace(column.DefaultValue))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Для режима InheritedIfEmpty требуется DefaultValue."));
                }
            }

            return result;
        }

        private static void ValidateRelations(IReadOnlyCollection<ExcelImportProfile> profiles, ExcelImportValidationResult result)
        {
            var profilesBySource = profiles.ToDictionary(x => x.SourceSelection.SourceName, StringComparer.OrdinalIgnoreCase);

            foreach (var profile in profiles)
            {
                if (profile.Relation.HasParent == false)
                    continue;

                if (string.Equals(profile.Relation.ParentSourceName, profile.SourceSelection.SourceName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Лист не может быть родителем сам для себя."));
                    continue;
                }

                if (profilesBySource.TryGetValue(profile.Relation.ParentSourceName, out var parentProfile) == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        $"Не выбран родительский лист \"{profile.Relation.ParentSourceName}\" для текущего импорта."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(profile.Relation.ParentKeyColumnName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Для связи с родителем не указана колонка ключа у родителя."));
                }
                else if (parentProfile.Columns.Any(x => string.Equals(x.HeaderName, profile.Relation.ParentKeyColumnName, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        profile.Relation.ParentKeyColumnName,
                        $"На родительском листе \"{profile.Relation.ParentSourceName}\" не найдена колонка ключа \"{profile.Relation.ParentKeyColumnName}\"."));
                }

                if (string.IsNullOrWhiteSpace(profile.Relation.ChildKeyColumnName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Для связи с родителем не указана колонка ключа у текущего листа."));
                }
                else if (profile.Columns.Any(x => string.Equals(x.HeaderName, profile.Relation.ChildKeyColumnName, StringComparison.OrdinalIgnoreCase)) == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        profile.Relation.ChildKeyColumnName,
                        $"На текущем листе не найдена колонка ключа \"{profile.Relation.ChildKeyColumnName}\"."));
                }
            }

            foreach (var profile in profiles)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = profile;

                while (current.Relation.HasParent)
                {
                    if (visited.Add(current.SourceSelection.SourceName) == false)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            profile.SourceSelection.SourceName,
                            string.Empty,
                            "Обнаружен циклический граф связей между листами."));
                        break;
                    }

                    if (profilesBySource.TryGetValue(current.Relation.ParentSourceName, out var parent) == false)
                        break;

                    current = parent;
                }
            }
        }

        private void ValidateRelationMatches(string filePath, IReadOnlyCollection<ExcelImportProfile> profiles, ExcelImportValidationResult result)
        {
            using var workbook = new XLWorkbook(filePath);
            var profilesBySource = profiles.ToDictionary(x => x.SourceSelection.SourceName, StringComparer.OrdinalIgnoreCase);

            foreach (var profile in profiles.Where(x => x.Relation.HasParent))
            {
                if (profilesBySource.TryGetValue(profile.Relation.ParentSourceName, out var parentProfile) == false)
                    continue;

                var parentRange = _sourceReader.ResolveRange(workbook, parentProfile.SourceSelection);
                var childRange = _sourceReader.ResolveRange(workbook, profile.SourceSelection);
                if (parentRange == null || childRange == null)
                    continue;

                var parentKeyColumn = parentProfile.Columns.FirstOrDefault(x =>
                    string.Equals(x.HeaderName, profile.Relation.ParentKeyColumnName, StringComparison.OrdinalIgnoreCase));
                var childKeyColumn = profile.Columns.FirstOrDefault(x =>
                    string.Equals(x.HeaderName, profile.Relation.ChildKeyColumnName, StringComparison.OrdinalIgnoreCase));

                if (parentKeyColumn == null || childKeyColumn == null)
                    continue;

                var parentKeys = parentRange.RowsUsed()
                    .Skip(1)
                    .Select(row => ExcelRelationKeyHelper.Normalize(row.Cell(parentKeyColumn.ColumnIndex).GetString()))
                    .Where(x => string.IsNullOrWhiteSpace(x) == false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var childKeys = childRange.RowsUsed()
                    .Skip(1)
                    .Select(row => ExcelRelationKeyHelper.Normalize(row.Cell(childKeyColumn.ColumnIndex).GetString()))
                    .Where(x => string.IsNullOrWhiteSpace(x) == false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (childKeys.Count == 0)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        profile.Relation.ChildKeyColumnName,
                        "В колонке связи дочернего листа нет ни одного непустого значения."));
                    continue;
                }

                var hasMatches = childKeys.Any(childKey => parentKeys.Any(parentKey => ExcelRelationKeyHelper.AreEqual(parentKey, childKey)));
                if (hasMatches)
                    continue;

                var sampleParentKeys = parentKeys.Take(5).ToList();
                var sampleChildKeys = childKeys.Take(5).ToList();
                var details = $"Родительские значения: {string.Join(", ", sampleParentKeys)}. Дочерние значения: {string.Join(", ", sampleChildKeys)}.";

                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    profile.Relation.ChildKeyColumnName,
                    $"Не найдено ни одной связи с родительским листом \"{profile.Relation.ParentSourceName}\" по ключам \"{profile.Relation.ParentKeyColumnName}\" -> \"{profile.Relation.ChildKeyColumnName}\". {details}"));
            }
        }

        private void ValidateAttributeColumn(
            ExcelImportProfile profile,
            IXLRange range,
            ExcelImportColumnProfile column,
            ExcelImportInheritanceInfo inheritanceInfo,
            ExcelImportValidationResult result)
        {
            if (column.ValueMode == ExcelImportValueMode.InheritedConstant)
            {
                if (inheritanceInfo.HasResolvedParentValue == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Для режима InheritedConstant требуется DefaultValue или одно уникальное непустое значение по всему столбцу."));
                    return;
                }

                if (string.IsNullOrWhiteSpace(column.DefaultValue) == false
                    && inheritanceInfo.DistinctNonEmptyValues.Count > 0
                    && inheritanceInfo.DistinctNonEmptyValues.Any(x => string.Equals(x, inheritanceInfo.ResolvedParentValue, StringComparison.Ordinal) == false))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Значения столбца конфликтуют с DefaultValue для режима InheritedConstant."));
                    return;
                }

                if (_dataTypeDetector.IsValueCompatibleWithDataType(inheritanceInfo.ResolvedParentValue!, column.DataTypeNodeName) == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        $"Значение \"{inheritanceInfo.ResolvedParentValue}\" не соответствует типу \"{column.DataTypeNodeName}\"."));
                }

                return;
            }

            if (column.ValueMode == ExcelImportValueMode.InheritedIfEmpty
                && string.IsNullOrWhiteSpace(column.DefaultValue) == false
                && _dataTypeDetector.IsValueCompatibleWithDataType(column.DefaultValue, column.DataTypeNodeName) == false)
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    column.HeaderName,
                    $"DefaultValue \"{column.DefaultValue}\" не соответствует типу \"{column.DataTypeNodeName}\"."));
            }

            var rows = range.RowsUsed().ToList();
            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var value = rows[rowIndex].Cell(column.ColumnIndex).GetString().Trim();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (_dataTypeDetector.IsValueCompatibleWithDataType(value, column.DataTypeNodeName))
                    continue;

                result.Errors.Add(new ExcelImportValidationError
                {
                    SourceName = profile.SourceSelection.SourceName,
                    RowNumber = rows[rowIndex].RowNumber(),
                    ColumnName = column.HeaderName,
                    Value = value,
                    Message = $"Значение \"{value}\" не соответствует типу \"{column.DataTypeNodeName}\"."
                });
            }
        }

        private static void ValidateSequenceColumn(ExcelImportProfile profile, IXLRange range, ExcelImportValidationResult result)
        {
            var sequenceColumn = profile.Columns.FirstOrDefault(x => x.Role == ExcelImportColumnRole.SystemSequence);
            if (sequenceColumn == null)
                return;

            var rows = range.RowsUsed().ToList();
            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var sequenceValue = rows[rowIndex].Cell(sequenceColumn.ColumnIndex).GetString().Trim();
                if (string.IsNullOrWhiteSpace(sequenceValue))
                    continue;

                if (long.TryParse(sequenceValue, out _))
                    continue;

                result.Errors.Add(new ExcelImportValidationError
                {
                    SourceName = profile.SourceSelection.SourceName,
                    RowNumber = rows[rowIndex].RowNumber(),
                    ColumnName = sequenceColumn.HeaderName,
                    Value = sequenceValue,
                    Message = $"Значение \"{sequenceValue}\" не соответствует типу \"Целое число\" для последовательности."
                });
            }
        }

        private static void ValidateSingleSystemColumn(
            ExcelImportProfile profile,
            IEnumerable<ExcelImportColumnProfile> columns,
            ExcelImportColumnRole role,
            ExcelImportValidationResult result)
        {
            var matchedColumns = columns.Where(x => x.Role == role).ToList();
            if (matchedColumns.Count <= 1)
                return;

            foreach (var column in matchedColumns.Skip(1))
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    column.HeaderName,
                    $"Назначено больше одной колонки с ролью {role}."));
            }
        }

        private void ValidateRootScopeConflicts(string filePath, IEnumerable<ExcelImportProfile> profiles, ExcelImportValidationResult result)
        {
            using var workbook = new XLWorkbook(filePath);

            var rootScopedColumns = profiles
                .SelectMany(profile => profile.Columns
                    .Where(column => column.Role == ExcelImportColumnRole.Attribute && column.DefinitionScope == ExcelImportDefinitionScope.Root)
                    .Select(column => new
                    {
                        profile.SourceSelection.SourceName,
                        Column = column,
                        Inheritance = ResolveInheritanceSafely(workbook, profile, column)
                    }))
                .GroupBy(x => x.Column.HeaderName, StringComparer.OrdinalIgnoreCase);

            foreach (var group in rootScopedColumns)
            {
                var first = group.First().Column;
                var firstResolvedValue = group.First().Inheritance?.ResolvedParentValue ?? string.Empty;
                var hasConflict = group.Skip(1).Any(item =>
                    item.Column.DataTypeNodeName != first.DataTypeNodeName
                    || item.Column.IsCollectionValue != first.IsCollectionValue
                    || item.Column.Visibility != first.Visibility
                    || item.Column.Override != first.Override
                    || item.Column.ValueMode != first.ValueMode
                    || string.Equals(item.Inheritance?.ResolvedParentValue ?? string.Empty, firstResolvedValue, StringComparison.Ordinal) == false);

                if (hasConflict)
                {
                    foreach (var item in group)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            item.SourceName,
                            item.Column.HeaderName,
                            "Корневой атрибут с таким именем определен на нескольких листах с конфликтующими настройками."));
                    }
                }
            }
        }

        private ExcelImportInheritanceInfo? ResolveInheritanceSafely(XLWorkbook workbook, ExcelImportProfile profile, ExcelImportColumnProfile column)
        {
            var range = _sourceReader.ResolveRange(workbook, profile.SourceSelection);
            return range == null ? null : _inheritanceResolver.Resolve(range, column);
        }

        private static ExcelImportValidationError CreateConfigurationError(string sourceName, string columnName, string message)
        {
            return new ExcelImportValidationError
            {
                SourceName = sourceName,
                ColumnName = columnName,
                Message = message,
                IsConfigurationError = true
            };
        }
    }
}
