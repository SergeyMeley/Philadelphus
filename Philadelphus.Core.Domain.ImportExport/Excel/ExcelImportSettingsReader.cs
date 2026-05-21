using ClosedXML.Excel;
using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelImportSettingsReader : IExcelImportSettingsReader
    {
        private readonly IExcelImportSourceReader _sourceReader;

        public ExcelImportSettingsReader(IExcelImportSourceReader sourceReader)
        {
            _sourceReader = sourceReader;
        }

        public ExcelImportSettingsDocument Read(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            return Read(workbook);
        }

        private ExcelImportSettingsDocument Read(XLWorkbook workbook)
        {
            // Формат листа настроек табличный: первая строка - заголовки, дальше правила.
            // RuleType/ТипПравила принимает WorkbookDefault, WorksheetDefault или ColumnRule.
            var settingsWorksheet = workbook.Worksheets
                .FirstOrDefault(worksheet => _sourceReader.IsSettingsWorksheet(worksheet.Name));

            if (settingsWorksheet == null)
                return new ExcelImportSettingsDocument();

            var usedRange = settingsWorksheet.RangeUsed();
            if (usedRange == null)
                return new ExcelImportSettingsDocument();

            var rows = usedRange.RowsUsed().ToList();
            if (rows.Count <= 1)
                return new ExcelImportSettingsDocument();

            var headers = BuildHeaderMap(rows[0]);
            var document = new ExcelImportSettingsDocument();

            foreach (var row in rows.Skip(1))
            {
                if (IsEmptyRow(row))
                    continue;

                // Поддерживаем английские и русские заголовки, чтобы файл можно было подготовить без знания C# enum/property names.
                var item = new ExcelImportSettingsRowDto
                {
                    RuleType = GetString(row, headers, "RuleType", "ТипПравила", "Тип правила"),
                    SourceName = GetString(row, headers, "SourceName", "Лист", "Источник", "ИмяЛиста", "Имя листа"),
                    ParentSourceName = GetString(row, headers, "ParentSourceName", "РодительскийЛист", "Родительский лист"),
                    ParentKeyColumnName = GetString(row, headers, "ParentKeyColumnName", "КлючРодителя", "Ключ родителя"),
                    ChildKeyColumnName = GetString(row, headers, "ChildKeyColumnName", "КлючДочернегоЛиста", "Ключ дочернего листа"),
                    ColumnIndex = GetNullableInt(row, headers, "ColumnIndex", "НомерКолонки", "Номер колонки"),
                    HeaderName = GetString(row, headers, "HeaderName", "Заголовок", "Колонка", "ИмяКолонки", "Имя колонки"),
                    Role = GetColumnRole(row, headers, "Role", "Роль"),
                    DefinitionScope = GetEnum<ExcelImportDefinitionScope>(row, headers, "DefinitionScope", "Область", "ОбластьОпределения", "Область определения"),
                    ValueMode = GetEnum<ExcelImportValueMode>(row, headers, "ValueMode", "Наследование", "РежимЗначения", "Режим значения"),
                    Placement = GetEnum<ExcelImportPropertyPlacement>(row, headers, "Placement", "PropertyPlacement", "Размещение", "РазмещениеСвойства", "Размещение свойства"),
                    PropagationMode = GetEnum<ExcelImportValuePropagationMode>(row, headers, "PropagationMode", "ValuePropagationMode", "Распространение", "РаспространениеЗначения", "Распространение значения"),
                    EntityKind = GetEntityKind(row, headers, "EntityKind", "ТипСущности", "Тип сущности", "Материализация"),
                    DataTypeNodeName = GetString(row, headers, "DataTypeNodeName", "ТипДанных", "Тип данных"),
                    IsCollectionValue = GetNullableBool(row, headers, "IsCollectionValue", "Коллекция", "КоллекционноеЗначение", "Коллекционное значение"),
                    Visibility = GetEnum<VisibilityScope>(row, headers, "Visibility", "Видимость"),
                    Override = GetEnum<OverrideType>(row, headers, "Override", "Переопределение"),
                    Description = GetString(row, headers, "Description", "Описание"),
                    DefaultValue = GetString(row, headers, "DefaultValue", "ЗначениеПоУмолчанию", "Значение по умолчанию")
                };

                switch (NormalizeName(item.RuleType))
                {
                    case "workbookdefault":
                    case "настройкикниги":
                    case "поумолчаниюдлякниги":
                        document.WorkbookDefaults.Add(item);
                        break;
                    case "worksheetdefault":
                    case "настройкилиста":
                    case "поумолчаниюдлялиста":
                        document.WorksheetDefaults.Add(item);
                        break;
                    case "columnrule":
                    case "правилоколонки":
                    case "колонка":
                        document.ColumnRules.Add(item);
                        break;
                }
            }

            return document;
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var header = cell.GetFormattedString().Trim();
                if (string.IsNullOrWhiteSpace(header) || result.ContainsKey(header))
                    continue;

                result[header] = cell.Address.ColumnNumber;
                // Нормализованный ключ делает "Тип данных", "ТипДанных" и "тип_данных" эквивалентными.
                var normalizedHeader = NormalizeName(header);
                if (string.IsNullOrWhiteSpace(normalizedHeader) == false && result.ContainsKey(normalizedHeader) == false)
                {
                    result[normalizedHeader] = cell.Address.ColumnNumber;
                }
            }

            return result;
        }

        private static bool IsEmptyRow(IXLRangeRow row)
        {
            return row.CellsUsed().Any() == false
                || row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetFormattedString()));
        }

        private static string GetString(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
        {
            foreach (var headerName in headerNames)
            {
                if (headers.TryGetValue(headerName, out var columnNumber)
                    || headers.TryGetValue(NormalizeName(headerName), out columnNumber))
                {
                    return row.Cell(columnNumber).GetFormattedString().Trim();
                }
            }

            return string.Empty;
        }

        private static int? GetNullableInt(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
        {
            var value = GetString(row, headers, headerNames);
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private static bool? GetNullableBool(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
        {
            var value = GetString(row, headers, headerNames);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (bool.TryParse(value, out var parsed))
                return parsed;

            if (value == "1")
                return true;

            if (value == "0")
                return false;

            return null;
        }

        private static TEnum? GetEnum<TEnum>(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
            where TEnum : struct, Enum
        {
            var value = GetString(row, headers, headerNames);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<TEnum>(value, true, out var parsed))
                return parsed;

            foreach (var candidate in Enum.GetValues<TEnum>())
            {
                var member = typeof(TEnum).GetMember(candidate.ToString()).FirstOrDefault();
                var display = member?.GetCustomAttribute<DisplayAttribute>();
                if (string.Equals(display?.Name, value, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        private static ExcelImportColumnRole? GetColumnRole(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
        {
            var value = GetString(row, headers, headerNames);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (ExcelImportColumnRoleHelper.TryDetectMarkerRole(value, out var markerRole))
                return markerRole;

            if (Enum.TryParse<ExcelImportColumnRole>(value, true, out var parsed))
                return parsed;

            return NormalizeName(value) switch
            {
                "name" or "имя" or "название" or "systemname" => ExcelImportColumnRole.SystemName,
                "description" or "desc" or "описание" or "systemdescription" => ExcelImportColumnRole.SystemDescription,
                "sequence" or "порядок" or "systemsequence" => ExcelImportColumnRole.SystemSequence,
                "ignore" or "игнор" or "неимпортировать" or "пропустить" => ExcelImportColumnRole.Ignore,
                "attribute" or "атрибут" or "данные" => ExcelImportColumnRole.Attribute,
                _ => null
            };
        }

        private static ExcelImportEntityKind? GetEntityKind(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, params string[] headerNames)
        {
            var value = GetString(row, headers, headerNames);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<ExcelImportEntityKind>(value, true, out var parsed))
                return parsed;

            return NormalizeName(value) switch
            {
                "auto" or "авто" or "автоматически" => ExcelImportEntityKind.Leaf,
                "node" or "nodes" or "узел" or "узлы" => ExcelImportEntityKind.Node,
                "leaf" or "leaves" or "лист" or "листья" or "листок" or "вариант" or "варианты" => ExcelImportEntityKind.Leaf,
                _ => null
            };
        }

        private static string NormalizeName(string? value)
        {
            return new string((value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Where(ch => char.IsWhiteSpace(ch) == false && ch != '_' && ch != '-')
                .ToArray());
        }
    }
}
