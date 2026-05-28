using System;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    /// <summary>
    /// Единая таблица маркеров колонок для простого импорта, конструктора и relation-aware режима.
    /// Все новые aliases нужно добавлять сюда, а не дублировать switch по окнам или сервисам.
    /// </summary>
    public static class ExcelImportColumnRoleHelper
    {
        public static ExcelImportColumnRole DetectRole(string value)
        {
            return TryDetectMarkerRole(value, out var role)
                ? role
                : ExcelImportColumnRole.Attribute;
        }

        public static bool TryDetectMarkerRole(string value, out ExcelImportColumnRole role)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();

            // Любой неизвестный #маркер считаем Ignore: так служебные колонки в подготовленных Excel
            // не превращаются случайно в атрибуты Чубушника.
            role = normalized switch
            {
                "#name" => ExcelImportColumnRole.SystemName,
                "#имя" => ExcelImportColumnRole.SystemName,
                "#название" => ExcelImportColumnRole.SystemName,
                "#description" => ExcelImportColumnRole.SystemDescription,
                "#desc" => ExcelImportColumnRole.SystemDescription,
                "#описание" => ExcelImportColumnRole.SystemDescription,
                "#sequence" => ExcelImportColumnRole.SystemSequence,
                "#порядок" => ExcelImportColumnRole.SystemSequence,
                "#ignore" => ExcelImportColumnRole.Ignore,
                "#игнор" => ExcelImportColumnRole.Ignore,
                "#attribute" => ExcelImportColumnRole.Attribute,
                "#атрибут" => ExcelImportColumnRole.Attribute,
                _ when normalized.StartsWith("#", StringComparison.Ordinal) => ExcelImportColumnRole.Ignore,
                _ => ExcelImportColumnRole.Attribute
            };

            return normalized switch
            {
                "#name" or "#имя" or "#название" or
                "#description" or "#desc" or "#описание" or
                "#sequence" or "#порядок" or
                "#ignore" or "#игнор" or
                "#attribute" or "#атрибут" => true,
                _ when normalized.StartsWith("#", StringComparison.Ordinal) => true,
                _ => false
            };
        }
    }
}
