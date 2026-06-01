using System.Collections.Generic;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public interface IExcelImportProfileValidator
    {
        ExcelImportValidationResult ValidateConfiguration(ExcelImportProfile profile);

        ExcelImportValidationResult ValidateProfile(string filePath, ExcelImportProfile profile);

        ExcelImportValidationResult ValidateProfiles(string filePath, IEnumerable<ExcelImportProfile> profiles);
    }
}
