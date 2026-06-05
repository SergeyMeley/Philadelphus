using ClosedXML.Excel;
using FluentAssertions;
using Philadelphus.Infrastructure.ImportExport.Excel;

namespace Philadelphus.Tests.Domain.ImportExport
{
    public class ExcelImportLimitsTests
    {
        [Fact]
        public void ProcessFile_Rejects_File_Above_Size_Limit()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

            try
            {
                using (var stream = File.Create(filePath))
                {
                    stream.SetLength(ExcelImportLimits.MaxFileSizeBytes + 1);
                }

                var service = CreateConversionService();

                var act = () => service.ProcessFile(filePath, createNewRoot: true, rootNameInput: "Root");

                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*Файл Excel имеет размер*максимум разрешено*");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Fact]
        public void ProcessFile_Rejects_Source_Above_Row_Limit()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.AddWorksheet("Rows");
                    worksheet.Cell(1, 1).Value = "Name";
                    worksheet.Cell(ExcelImportLimits.MaxRowsPerSource + 1, 1).Value = "Too far";
                    workbook.SaveAs(filePath);
                }

                var service = CreateConversionService();

                var act = () => service.ProcessFile(filePath, createNewRoot: true, rootNameInput: "Root");

                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*содержит*строк*максимум разрешено*");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static ConversionService CreateConversionService()
        {
            var sourceReader = new ExcelImportSourceReader();
            return new ConversionService(
                new ExcelDataTypeDetector(),
                sourceReader,
                new ExcelImportProfileResolver(new ExcelImportSettingsReader(sourceReader)));
        }
    }
}
