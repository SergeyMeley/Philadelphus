using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Helpers;
using Philadelphus.Core.Domain.TablesExport.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Philadelphus.Core.Domain.TablesExport.Services
{
    /// <summary>
    /// Сервис экспорта таблиц в файл
    /// </summary>
    public sealed class OpenXmlExcelTablesExportService : ITablesExportService
    {
        private readonly INotificationService _notificationService;

        private const uint MaxExcelRows = 1_048_576;

        /// <summary>
        /// Наименование формата
        /// </summary>
        public string FormatName => "Excel";

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string FileExtension => "xlsx";

        /// <summary>
        /// Сервис экспорта таблиц в файл
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений</param>
        public OpenXmlExcelTablesExportService(
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(notificationService);

            _notificationService = notificationService;
        }

        /// <summary>
        /// Экспортировать таблицу в файл
        /// </summary>
        /// <typeparam name="T">Тип данных строки</typeparam>
        /// <param name="data">Данные для экспорта</param>
        /// <param name="columns">Колонки таблицы</param>
        /// <param name="reportName">Наименование отчета</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        public async Task<string> ExportAsync<T>(
            IAsyncEnumerable<T> data,
            IReadOnlyList<TableExportColumn<T>> columns,
            string reportName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(columns);

            if (columns.Count == 0)
                throw new ArgumentException("Не задан список колонок для экспорта.", nameof(columns));

            var path = TablesExportPathBuilder.BuildExportPath(reportName, FileExtension);

            using var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            AddStyles(workbookPart);

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(worksheetPart))
            {
                writer.WriteStartElement(new Worksheet());

                WriteSheetViews(writer);
                WriteColumns(writer, columns);
                writer.WriteStartElement(new SheetData());

                uint rowIndex = 1;

                WriteHeaderRow(writer, columns, rowIndex);
                rowIndex++;

                await foreach (var item in data.WithCancellation(cancellationToken))
                {
                    if (rowIndex > MaxExcelRows)
                        throw new InvalidOperationException(
                            $"Excel поддерживает максимум {MaxExcelRows} строк на лист.");

                    WriteDataRow(writer, item, columns, rowIndex);
                    rowIndex++;
                }

                writer.WriteEndElement(); // SheetData

                var lastColumn = GetExcelColumnName(columns.Count);
                var lastRow = rowIndex > 1 ? rowIndex - 1 : 1;

                writer.WriteElement(new AutoFilter
                {
                    Reference = $"A1:{lastColumn}{lastRow}"
                });

                writer.WriteEndElement(); // Worksheet
            }

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Report"
            });

            workbookPart.Workbook.Save();

            _notificationService.SendTextMessage<ITablesExportService>(
                $"Экспорт отчета '{reportName}' успешно выполнен. Сохранено в '{path}'",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return path;
        }

        private static void WriteHeaderRow<T>(
            OpenXmlWriter writer,
            IReadOnlyList<TableExportColumn<T>> columns,
            uint rowIndex)
        {
            writer.WriteStartElement(new Row { RowIndex = rowIndex });

            for (var i = 0; i < columns.Count; i++)
            {
                WriteTextCell(
                    writer,
                    GetExcelColumnName(i + 1) + rowIndex,
                    columns[i].Header,
                    styleIndex: 1);
            }

            writer.WriteEndElement();
        }

        private static void WriteDataRow<T>(
            OpenXmlWriter writer,
            T item,
            IReadOnlyList<TableExportColumn<T>> columns,
            uint rowIndex)
        {
            writer.WriteStartElement(new Row { RowIndex = rowIndex });

            for (var i = 0; i < columns.Count; i++)
            {
                var value = columns[i].ValueSelector(item);
                var cellReference = GetExcelColumnName(i + 1) + rowIndex;

                WriteTypedCell(writer, cellReference, value);
            }

            writer.WriteEndElement();
        }

        private static void WriteTypedCell(OpenXmlWriter writer, string cellReference, object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteTextCell(writer, cellReference, "");
                return;
            }

            switch (value)
            {
                case byte or short or int or long or float or double or decimal:
                    writer.WriteElement(new Cell
                    {
                        CellReference = cellReference,
                        DataType = CellValues.Number,
                        CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture))
                    });
                    break;

                case bool boolValue:
                    writer.WriteElement(new Cell
                    {
                        CellReference = cellReference,
                        DataType = CellValues.Boolean,
                        CellValue = new CellValue(boolValue ? "1" : "0")
                    });
                    break;

                case DateTime dateTime:
                    writer.WriteElement(new Cell
                    {
                        CellReference = cellReference,
                        StyleIndex = 2,
                        DataType = CellValues.Number,
                        CellValue = new CellValue(dateTime.ToOADate().ToString(CultureInfo.InvariantCulture))
                    });
                    break;

                case DateTimeOffset dateTimeOffset:
                    writer.WriteElement(new Cell
                    {
                        CellReference = cellReference,
                        StyleIndex = 2,
                        DataType = CellValues.Number,
                        CellValue = new CellValue(dateTimeOffset.DateTime.ToOADate().ToString(CultureInfo.InvariantCulture))
                    });
                    break;

                default:
                    WriteTextCell(writer, cellReference, value.ToString() ?? "");
                    break;
            }
        }

        private static void WriteTextCell(
            OpenXmlWriter writer,
            string cellReference,
            string value,
            uint styleIndex = 0)
        {
            writer.WriteElement(new Cell
            {
                CellReference = cellReference,
                StyleIndex = styleIndex,
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value ?? ""))
            });
        }

        private static void WriteSheetViews(OpenXmlWriter writer)
        {
            writer.WriteStartElement(new SheetViews());
            writer.WriteStartElement(new SheetView { WorkbookViewId = 0 });

            writer.WriteElement(new Pane
            {
                VerticalSplit = 1,
                TopLeftCell = "A2",
                ActivePane = PaneValues.BottomLeft,
                State = PaneStateValues.Frozen
            });

            writer.WriteEndElement(); // SheetView
            writer.WriteEndElement(); // SheetViews
        }

        private static void WriteColumns<T>(
            OpenXmlWriter writer,
            IReadOnlyList<TableExportColumn<T>> columns)
        {
            writer.WriteStartElement(new Columns());

            for (var i = 0; i < columns.Count; i++)
            {
                writer.WriteElement(new Column
                {
                    Min = (uint)(i + 1),
                    Max = (uint)(i + 1),
                    Width = columns[i].Width,
                    CustomWidth = true
                });
            }

            writer.WriteEndElement();
        }

        private static void AddStyles(WorkbookPart workbookPart)
        {
            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

            stylesPart.Stylesheet = new Stylesheet(
                new Fonts(
                    new Font(),
                    new Font(new Bold())
                ),
                new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }),
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
                ),
                new Borders(new Border()),
                new CellFormats(
                    new CellFormat(),   // 0 default
                    new CellFormat      // 1 header
                    {
                        FontId = 1,
                        ApplyFont = true
                    },
                    new CellFormat      // 2 date
                    {
                        NumberFormatId = 22,
                        ApplyNumberFormat = true
                    }
                )
            );

            stylesPart.Stylesheet.Save();
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}
