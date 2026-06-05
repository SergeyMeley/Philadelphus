using ClosedXML.Excel;
using System.IO.Compression;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public static class ExcelImportLimits
    {
        public const long MaxFileSizeBytes = 50L * 1024 * 1024;
        public const long MaxArchiveUncompressedSizeBytes = 250L * 1024 * 1024;
        public const long MaxArchiveEntrySizeBytes = 100L * 1024 * 1024;
        public const int MaxArchiveEntryCount = 4096;
        public const int MaxWorksheetCount = 100;
        public const int MaxImportSourceCount = 200;
        public const int MaxRowsPerSource = 100_000;
        public const int MaxColumnsPerSource = 256;
        public const long MaxCellsPerSource = 1_000_000;

        internal static XLWorkbook OpenWorkbook(string filePath)
        {
            ValidateFileBeforeOpen(filePath);

            var workbook = new XLWorkbook(filePath);
            try
            {
                ValidateWorkbook(workbook);
                return workbook;
            }
            catch
            {
                workbook.Dispose();
                throw;
            }
        }

        internal static void ValidateWorkbook(XLWorkbook workbook)
        {
            ArgumentNullException.ThrowIfNull(workbook);

            if (workbook.Worksheets.Count > MaxWorksheetCount)
            {
                throw new InvalidOperationException(
                    $"Excel-файл содержит {workbook.Worksheets.Count} листов, максимум разрешено {MaxWorksheetCount}.");
            }
        }

        internal static void ValidateSourceCount(int sourceCount)
        {
            if (sourceCount > MaxImportSourceCount)
            {
                throw new InvalidOperationException(
                    $"Excel-импорт содержит {sourceCount} источников данных, максимум разрешено {MaxImportSourceCount}.");
            }
        }

        internal static void ValidateRange(IXLRange? range, string sourceName)
        {
            if (range == null)
            {
                return;
            }

            var firstRow = range.FirstRowUsed();
            var lastRow = range.LastRowUsed();
            var firstColumn = range.FirstColumnUsed();
            var lastColumn = range.LastColumnUsed();

            if (firstRow == null || lastRow == null || firstColumn == null || lastColumn == null)
            {
                return;
            }

            var rowCount = lastRow.RowNumber() - firstRow.RowNumber() + 1;
            var columnCount = lastColumn.ColumnNumber() - firstColumn.ColumnNumber() + 1;
            var cellCount = (long)rowCount * columnCount;
            var displayName = string.IsNullOrWhiteSpace(sourceName) ? "источник Excel" : sourceName;

            if (rowCount > MaxRowsPerSource)
            {
                throw new InvalidOperationException(
                    $"Источник Excel «{displayName}» содержит {rowCount} строк, максимум разрешено {MaxRowsPerSource}.");
            }

            if (columnCount > MaxColumnsPerSource)
            {
                throw new InvalidOperationException(
                    $"Источник Excel «{displayName}» содержит {columnCount} колонок, максимум разрешено {MaxColumnsPerSource}.");
            }

            if (cellCount > MaxCellsPerSource)
            {
                throw new InvalidOperationException(
                    $"Источник Excel «{displayName}» содержит {cellCount} ячеек, максимум разрешено {MaxCellsPerSource}.");
            }
        }

        private static void ValidateFileBeforeOpen(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == false)
            {
                throw new FileNotFoundException("Файл Excel для импорта не найден.", filePath);
            }

            if (fileInfo.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"Файл Excel имеет размер {fileInfo.Length} байт, максимум разрешено {MaxFileSizeBytes} байт.");
            }

            ValidateArchiveEnvelope(filePath);
        }

        private static void ValidateArchiveEnvelope(string filePath)
        {
            try
            {
                using var archive = ZipFile.OpenRead(filePath);
                long uncompressedSize = 0;
                var entryCount = 0;

                foreach (var entry in archive.Entries)
                {
                    entryCount++;
                    if (entryCount > MaxArchiveEntryCount)
                    {
                        throw new InvalidOperationException(
                            $"Excel-файл содержит больше {MaxArchiveEntryCount} элементов архива.");
                    }

                    if (entry.Length > MaxArchiveEntrySizeBytes)
                    {
                        throw new InvalidOperationException(
                            $"Элемент архива Excel «{entry.FullName}» имеет размер {entry.Length} байт, максимум разрешено {MaxArchiveEntrySizeBytes} байт.");
                    }

                    uncompressedSize += entry.Length;
                    if (uncompressedSize > MaxArchiveUncompressedSizeBytes)
                    {
                        throw new InvalidOperationException(
                            $"Распакованный размер Excel-файла превышает {MaxArchiveUncompressedSizeBytes} байт.");
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidOperationException("Файл Excel не является корректным XLSX-архивом.", ex);
            }
        }
    }
}
