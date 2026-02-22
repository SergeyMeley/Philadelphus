using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ConversionService
    {
        // Эмуляция получения корней из хранилища "Чубушник"
        public List<string> GetExistingRootsFromStorage()
        {
            // В реальном приложении здесь будет вызов API или БД
            return new List<string> { "Склад", "Сотрудники", "Клиенты", "Архив" };
        }

        public string ProcessFile(string filePath, bool createNewRoot, string rootNameInput)
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

                foreach (var worksheet in workbook.Worksheets)
                {
                    // 1. Создаем Узел (TreeNode) для каждого Листа Excel
                    var node = new TreeNodeExportDTO
                    {
                        Name = worksheet.Name,
                        Description = $"Импортировано с листа «{worksheet.Name}»",
                        OwningRootName = finalRootName,
                        Attributes = new List<AttributeExportDTO>()
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

                            node.Attributes.Add(new AttributeExportDTO(headerName, $"Импортировано из колонки «{headerName}»")
                            {
                                DataTypeNodeName = dataType,
                                ValueLeaveName = null
                            });
                        }
                    }
                    allNodes.Add(node);

                    // 2. Создаем Листья (TreeLeaves) для каждой строки данных
                    var dataRows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок
                    int rowIndex = 2; // Нумерация строк Excel (1-based)

                    var allLeaves = new List<TreeLeaveExportDTO>();

                    foreach (var row in dataRows)
                    {
                        var leaf = new TreeLeaveExportDTO(rowIndex.ToString(), $"Импортировано из строки «{rowIndex}»", worksheet.Name);

                        int colIndex = 1;
                        foreach (var cell in row.CellsUsed())
                        {
                            // Находим описание атрибута, созданное выше
                            if (colIndex <= node.Attributes.Count)
                            {
                                var attrDef = node.Attributes[colIndex - 1];
                                string cellValue = cell.GetString();

                                leaf.Attributes.Add(new AttributeExportDTO(attrDef.Name, attrDef.Description)
                                {
                                    DataTypeNodeName = attrDef.DataTypeNodeName,
                                    ValueLeaveName = cellValue // Значение ячейки
                                });
                            }
                            colIndex++;
                        }
                        allLeaves.Add(leaf);
                        rowIndex++;
                    }

                    foreach (var nodeItem in allNodes)
                    {
                        nodeItem.ChildLeaves.AddRange(allLeaves.Where(x => x.OwningNodeName == nodeItem.Name));
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