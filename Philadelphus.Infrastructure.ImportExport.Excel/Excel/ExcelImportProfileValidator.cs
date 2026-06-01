using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphus.Infrastructure.ImportExport.Excel
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

            // Сначала проверяем саму схему связей между листами, а уже потом валидируем содержимое колонок.
            ValidateRelations(profileList, result);

            if (result.HasErrors == false)
            {
                // Если схема связи корректна, дополнительно убеждаемся, что на реальных данных есть хотя бы одно совпадение ключей.
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

            ValidateSingleSystemColumn(profile, columns, ExcelImportColumnRole.SystemDescription, result);
            ValidateSingleSystemColumn(profile, columns, ExcelImportColumnRole.SystemSequence, result);

            // Лист может быть валидным без атрибутов: например подготовленный справочник
            // только с #Name/#Описание/#Sequence. Проверяем наличие хотя бы одной импортируемой колонки.
            if (columns.Any(x => x.Role != ExcelImportColumnRole.Ignore) == false)
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    string.Empty,
                    "Не выбрано ни одной импортируемой колонки."));
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

                if (column.Placement == ExcelImportPropertyPlacement.Root
                    && column.PropagationMode == ExcelImportValuePropagationMode.ParentFallbackIfEmpty)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Свойство на Root не может использовать ParentFallbackIfEmpty: у root нет родителя."));
                }

                if (column.PropagationMode == ExcelImportValuePropagationMode.ParentFallbackIfEmpty
                    && string.IsNullOrWhiteSpace(column.DefaultValue))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Для ParentFallbackIfEmpty нужен DefaultValue или одноименное свойство на родительской сущности."));
                }
            }

            return result;
        }

        private static void ValidateRelations(IReadOnlyCollection<ExcelImportProfile> profiles, ExcelImportValidationResult result)
        {
            var profilesBySource = profiles.ToDictionary(x => x.SourceSelection.SourceName, StringComparer.OrdinalIgnoreCase);
            var childProfilesByParent = profiles
                .Where(x => x.Relation.HasParent)
                .GroupBy(x => x.Relation.ParentSourceName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var profile in profiles)
            {
                if (profile.EntityKind == ExcelImportEntityKind.Leaf
                    && childProfilesByParent.ContainsKey(profile.SourceSelection.SourceName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Сущность настроена как Leaf, но у нее есть дочерние связи. Relation не меняет EntityKind; выберите Node явно."));
                }

                if (profile.Relation.HasParent == false)
                    continue;

                // Защита от очевидных ошибок настройки: самоссылка, невыбранный лист, отсутствующие ключевые колонки.
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
                else
                {
                    var parentKeyColumn = parentProfile.Columns.FirstOrDefault(x =>
                        string.Equals(x.HeaderName, profile.Relation.ParentKeyColumnName, StringComparison.OrdinalIgnoreCase));
                    if (parentKeyColumn == null)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            profile.SourceSelection.SourceName,
                            profile.Relation.ParentKeyColumnName,
                            $"На родительском листе \"{profile.Relation.ParentSourceName}\" не найдена колонка ключа \"{profile.Relation.ParentKeyColumnName}\"."));
                    }
                    else if (parentKeyColumn.Role == ExcelImportColumnRole.Ignore)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            profile.SourceSelection.SourceName,
                            profile.Relation.ParentKeyColumnName,
                            $"Колонка ключа родителя \"{profile.Relation.ParentKeyColumnName}\" отключена через Ignore и не может участвовать в связи."));
                    }
                }

                if (string.IsNullOrWhiteSpace(profile.Relation.ChildKeyColumnName))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        string.Empty,
                        "Для связи с родителем не указана колонка ключа у текущего листа."));
                }
                else
                {
                    var childKeyColumn = profile.Columns.FirstOrDefault(x =>
                        string.Equals(x.HeaderName, profile.Relation.ChildKeyColumnName, StringComparison.OrdinalIgnoreCase));
                    if (childKeyColumn == null)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            profile.SourceSelection.SourceName,
                            profile.Relation.ChildKeyColumnName,
                            $"На текущем листе не найдена колонка ключа \"{profile.Relation.ChildKeyColumnName}\"."));
                    }
                    else if (childKeyColumn.Role == ExcelImportColumnRole.Ignore)
                    {
                        result.Errors.Add(CreateConfigurationError(
                            profile.SourceSelection.SourceName,
                            profile.Relation.ChildKeyColumnName,
                            $"Колонка ключа текущего листа \"{profile.Relation.ChildKeyColumnName}\" отключена через Ignore и не может участвовать в связи."));
                    }
                }
            }

            foreach (var profile in profiles)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = profile;

                // Отдельно проверяем, что пользователь не собрал цикл вида A -> B -> C -> A.
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

                // Сравниваем уже нормализованные значения ключей, чтобы отличия формата Excel не ломали связь.
                var parentKeyValues = ExcelImportRangeHelper.GetDataRows(parentRange, parentProfile.DataStartRowOffset)
                    .Select(row => new
                    {
                        RowNumber = row.RowNumber(),
                        Value = ExcelRelationKeyHelper.Normalize(ExcelImportRangeHelper.GetCellText(row, parentKeyColumn.ColumnIndex))
                    })
                    .Where(x => string.IsNullOrWhiteSpace(x.Value) == false)
                    .ToList();
                var duplicatedParentKeys = parentKeyValues
                    .GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Count() > 1)
                    .Take(5)
                    .Select(x => x.Key)
                    .ToList();
                if (duplicatedParentKeys.Count > 0)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.Relation.ParentSourceName,
                        profile.Relation.ParentKeyColumnName,
                        $"Колонка ключа родителя содержит повторяющиеся значения: {string.Join(", ", duplicatedParentKeys)}. Родительский ключ должен однозначно определять строку."));
                }

                var parentKeys = parentKeyValues
                    .Select(x => x.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (parentKeys.Count == 0)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.Relation.ParentSourceName,
                        profile.Relation.ParentKeyColumnName,
                        "В колонке связи родительского листа нет ни одного непустого значения."));
                    continue;
                }

                var childKeys = ExcelImportRangeHelper.GetDataRows(childRange, profile.DataStartRowOffset)
                    .Select(row => ExcelRelationKeyHelper.Normalize(ExcelImportRangeHelper.GetCellText(row, childKeyColumn.ColumnIndex)))
                    .Where(x => string.IsNullOrWhiteSpace(x) == false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (childKeys.Count == 0)
                    continue;

                var hasAnyMatch = childKeys.Any(childKey =>
                    parentKeys.Any(parentKey => ExcelRelationKeyHelper.AreEqual(parentKey, childKey)));

                if (hasAnyMatch == false)
                {
                    var sampleParentKeys = parentKeys.Take(5).ToList();
                    var sampleChildKeys = childKeys.Take(5).ToList();
                    var details = $"Родительские значения: {string.Join(", ", sampleParentKeys)}. Дочерние значения: {string.Join(", ", sampleChildKeys)}.";

                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        profile.Relation.ChildKeyColumnName,
                        $"Связь \"{profile.Relation.ParentSourceName}\" -> \"{profile.SourceSelection.SourceName}\" не дала ни одного совпадения по ключам \"{profile.Relation.ParentKeyColumnName}\" -> \"{profile.Relation.ChildKeyColumnName}\". Проверьте выбранные колонки связи. {details}"));
                    continue;
                }

                // Relation описывает опциональную ассоциацию между сущностями.
                // Родитель без дочерних строк и дочерняя строка без найденного родителя не являются ошибкой схемы:
                // materializer связывает совпавшие строки, а несовпавшие child-строки не поднимает в root.
            }
        }

        private void ValidateAttributeColumn(
            ExcelImportProfile profile,
            IXLRange range,
            ExcelImportColumnProfile column,
            ExcelImportInheritanceInfo inheritanceInfo,
            ExcelImportValidationResult result)
        {
            if (column.PropagationMode == ExcelImportValuePropagationMode.ParentConstant)
            {
                if (inheritanceInfo.HasResolvedParentValue == false)
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Для ParentConstant требуется DefaultValue или одно уникальное непустое значение по всему столбцу."));
                    return;
                }

                if (string.IsNullOrWhiteSpace(column.DefaultValue) == false
                    && inheritanceInfo.DistinctNonEmptyValues.Count > 0
                    && inheritanceInfo.DistinctNonEmptyValues.Any(x => string.Equals(x, inheritanceInfo.ResolvedParentValue, StringComparison.Ordinal) == false))
                {
                    result.Errors.Add(CreateConfigurationError(
                        profile.SourceSelection.SourceName,
                        column.HeaderName,
                        "Значения столбца конфликтуют с DefaultValue для ParentConstant."));
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

            if (column.PropagationMode == ExcelImportValuePropagationMode.ParentFallbackIfEmpty
                && string.IsNullOrWhiteSpace(column.DefaultValue) == false
                && _dataTypeDetector.IsValueCompatibleWithDataType(column.DefaultValue, column.DataTypeNodeName) == false)
            {
                result.Errors.Add(CreateConfigurationError(
                    profile.SourceSelection.SourceName,
                    column.HeaderName,
                    $"DefaultValue \"{column.DefaultValue}\" не соответствует типу \"{column.DataTypeNodeName}\"."));
            }

            // Валидация должна читать те же data rows, что и import pipeline, иначе строка маркеров
            // начнет проверяться как пользовательские данные.
            var rows = ExcelImportRangeHelper.GetDataRows(range, profile.DataStartRowOffset);
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var value = ExcelImportRangeHelper.GetCellText(rows[rowIndex], column.ColumnIndex);
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

            // Sequence тоже пропускает header/marker rows через DataStartRowOffset.
            var rows = ExcelImportRangeHelper.GetDataRows(range, profile.DataStartRowOffset);
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var sequenceValue = ExcelImportRangeHelper.GetCellText(rows[rowIndex], sequenceColumn.ColumnIndex);
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
                    .Where(column => column.Role == ExcelImportColumnRole.Attribute
                        && column.Placement == ExcelImportPropertyPlacement.Root)
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
                    || item.Column.PropagationMode != first.PropagationMode
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

        private static bool RowHasImportableData(IXLRangeRow row, IEnumerable<ExcelImportColumnProfile> columns)
        {
            return columns
                .Where(x => x.Role != ExcelImportColumnRole.Ignore)
                .Any(x => string.IsNullOrWhiteSpace(ExcelImportRangeHelper.GetCellText(row, x.ColumnIndex)) == false);
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
