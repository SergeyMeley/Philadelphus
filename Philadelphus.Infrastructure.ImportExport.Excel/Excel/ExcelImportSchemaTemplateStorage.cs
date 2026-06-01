using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Infrastructure.ImportExport.Excel
{
    public class ExcelImportSchemaTemplateStorage : IExcelImportSchemaTemplateStorage
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public void Save(string filePath, ExcelImportSchema schema)
        {
            var json = JsonSerializer.Serialize(schema, Options);
            File.WriteAllText(filePath, json);
        }

        public ExcelImportSchema Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var schema = JsonSerializer.Deserialize<ExcelImportSchema>(json, Options)
                ?? new ExcelImportSchema();

            schema.Sheets ??= new System.Collections.Generic.List<ExcelImportSheetSchema>();
            schema.Relations ??= new System.Collections.Generic.List<ExcelImportRelationSchema>();

            foreach (var sheet in schema.Sheets)
            {
                sheet.Profile ??= new ExcelImportProfile();
                sheet.Profile.Columns ??= new System.Collections.Generic.List<ExcelImportColumnProfile>();
                sheet.Profile.SourceSelection ??= new ExcelImportSourceSelection
                {
                    SourceName = sheet.SourceName,
                    SourceType = sheet.SourceType
                };
                if (string.IsNullOrWhiteSpace(sheet.Profile.SourceSelection.SourceName))
                {
                    sheet.Profile.SourceSelection.SourceName = sheet.SourceName;
                    sheet.Profile.SourceSelection.SourceType = sheet.SourceType;
                }
                sheet.Profile.Relation ??= new ExcelImportRelationProfile();
            }

            ExcelImportSchemaNormalizer.ApplyRelationProjectionToSheets(schema);
            ExcelImportSchemaNormalizer.RefreshRelationProjection(schema);
            return schema;
        }
    }
}
