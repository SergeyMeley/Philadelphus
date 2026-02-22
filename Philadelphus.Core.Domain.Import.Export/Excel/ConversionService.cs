using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ExcelToJsonConverter.Models;

namespace ExcelToJsonConverter.Services
{
    public class ConversionService
    {
        // Эмуляция получения корней из хранилища "Чубушник"
        public List<string> GetExistingRootsFromStorage()
        {
            // В реальном приложении здесь будет вызов API или БД
            return new List<string> { "Склад", "Сотрудники", "Клиенты", "Архив" };
        }

        public JsonTemplateObject ProcessFile(string filePath, bool createNewRoot, string rootNameInput)
        {
            var result = new JsonTemplateObject
            {
                TreeNodes = new List<TreeNodeDTO>(),
                TreeLeaves = new List<TreeLeaveDTO>()
            };

            string excelFileName = Path.GetFileNameWithoutExtension(filePath);

            // Логика определения имени корня
            string finalRootName = createNewRoot ? rootNameInput : rootNameInput;
            // Примечание: если createNewRoot = false, rootNameInput приходит из ComboBox (выбранный существующий корень)

            result.TreeRoot = new TreeRootDTO
            {
                Name = finalRootName,
                Description = $"Импортировано из книги «{excelFileName}»"
            };

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    // 1. Создаем Узел (TreeNode) для каждого Листа Excel
                    var node = new TreeNodeDTO
                    {
                        Name = worksheet.Name,
                        Description = $"Импортировано с листа «{worksheet.Name}»",
                        OwningRootName = finalRootName,
                        Attributes = new List<AttributeDTO>()
                    };

                    // Читаем заголовки (первая строка) для создания Атрибутов узла
                    var firstRow = worksheet.FirstRowUsed();
                    if (firstRow != null)
                    {
                        foreach (var cell in firstRow.CellsUsed())
                        {
                            string headerName = cell.GetString();
                            // Определяем тип данных эвристически (по первой ячейке с данными)
                            string dataType = DetermineDataType(worksheet, cell.Address.ColumnNumber);

                            node.Attributes.Add(new AttributeDTO
                            {
                                Name = headerName,
                                Description = $"Импортировано из колонки «{headerName}»",
                                DataTypeNodeName = dataType,
                                ValueLeaveName = null
                            });
                        }
                    }
                    result.TreeNodes.Add(node);

                    // 2. Создаем Листья (TreeLeaves) для каждой строки данных
                    var dataRows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок
                    int rowIndex = 2; // Нумерация строк Excel (1-based)

                    foreach (var row in dataRows)
                    {
                        var leaf = new TreeLeaveDTO
                        {
                            Name = rowIndex.ToString(), // Или генерация GUID
                            Description = $"Импортировано из строки «{rowIndex}»",
                            OwningNodeName = worksheet.Name,
                            Attributes = new List<AttributeDTO>()
                        };

                        int colIndex = 1;
                        foreach (var cell in row.CellsUsed())
                        {
                            // Находим описание атрибута, созданное выше
                            if (colIndex <= node.Attributes.Count)
                            {
                                var attrDef = node.Attributes[colIndex - 1];
                                string cellValue = cell.GetString();

                                leaf.Attributes.Add(new AttributeDTO
                                {
                                    Name = attrDef.Name,
                                    Description = attrDef.Description,
                                    DataTypeNodeName = attrDef.DataTypeNodeName,
                                    ValueLeaveName = cellValue // Значение ячейки
                                });
                            }
                            colIndex++;
                        }
                        result.TreeLeaves.Add(leaf);
                        rowIndex++;
                    }
                }
            }

            return result;
        }

        private string DetermineDataType(IXLWorksheet sheet, int colNumber)
        {
            // Простая эвристика: смотрим на тип первой заполненной ячейки в колонке (после заголовка)
            var firstDataCell = sheet.Column(colNumber).FirstCellUsed();
            if (firstDataCell == null) return "Строка";

            if (firstDataCell.DataType == XLDataType.Number) return "Число";
            if (firstDataCell.DataType == XLDataType.DateTime) return "Дата";
            if (firstDataCell.DataType == XLDataType.Boolean) return "Булево";

            return "Строка";
        }
    }
}