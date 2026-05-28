using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public class ExcelImportSessionState
    {
        private readonly ExcelPreviewService _previewService;
        private readonly IExcelImportSchemaBuilder _schemaBuilder;
        private readonly ExcelImportPipeline _importPipeline;

        public ExcelImportSessionState(
            ExcelPreviewService previewService,
            IExcelImportSchemaBuilder schemaBuilder,
            ExcelImportPipeline importPipeline)
        {
            _previewService = previewService;
            _schemaBuilder = schemaBuilder;
            _importPipeline = importPipeline;
        }

        public string SelectedFilePath { get; private set; } = string.Empty;

        public ExcelPreviewWorkbookInfo? WorkbookPreview { get; private set; }

        public ExcelImportSchema? Schema { get; private set; }

        public ExcelImportSheetSchema? CurrentSheet { get; set; }

        public void LoadWorkbook(string filePath, string rootName)
        {
            SelectedFilePath = filePath;
            WorkbookPreview = _previewService.GetWorkbookPreview(filePath);
            Schema = _schemaBuilder.CreateDraftSchema(filePath, rootName);
            CurrentSheet = Schema.Sheets.FirstOrDefault();
        }

        public void UseSchema(ExcelImportSchema schema)
        {
            Schema = schema;
            ExcelImportSchemaNormalizer.EnsureEditableState(Schema);
            ExcelImportSchemaNormalizer.RefreshRelationProjection(Schema);

            SelectedFilePath = Schema.SourceFilePath;
            WorkbookPreview = string.IsNullOrWhiteSpace(SelectedFilePath) || File.Exists(SelectedFilePath) == false
                ? null
                : _previewService.GetWorkbookPreview(SelectedFilePath);
            CurrentSheet = Schema.Sheets.FirstOrDefault();
        }

        public ExcelPreviewTable GetPreview(ExcelImportSheetSchema sheet)
        {
            return _previewService.GetPreview(SelectedFilePath, sheet.Profile.SourceSelection);
        }

        public ExcelImportSheetSchema? GetSheet(string? sourceName)
        {
            return Schema?.Sheets.FirstOrDefault(x => string.Equals(x.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));
        }

        public void SetAllSheetsEnabled(bool isEnabled)
        {
            if (Schema == null)
                return;

            foreach (var sheet in Schema.Sheets)
            {
                sheet.IsEnabled = isEnabled;
            }
        }

        public void SetSheetEnabled(string sourceName, bool isEnabled)
        {
            var sheet = GetSheet(sourceName);
            if (sheet != null)
            {
                sheet.IsEnabled = isEnabled;
            }
        }

        public void SyncRootSettings(bool createNewRoot, string rootName)
        {
            if (Schema == null)
                return;

            Schema.SourceFilePath = SelectedFilePath;
            Schema.CreateNewRoot = createNewRoot;
            Schema.RootName = rootName;
            ExcelImportSchemaNormalizer.RefreshRelationProjection(Schema);
        }

        public List<ExcelImportProfile> GetProfilesForExecution()
        {
            return Schema == null
                ? new List<ExcelImportProfile>()
                : _importPipeline.GetProfilesForExecution(Schema);
        }

        public ExcelImportValidationResult Validate()
        {
            return Schema == null
                ? new ExcelImportValidationResult()
                : _importPipeline.Validate(Schema);
        }

        public ExcelImportValidationResult ValidateProfileConfiguration(ExcelImportProfile profile)
        {
            return _previewService.ValidateProfileConfiguration(profile);
        }

        public string BuildJson()
        {
            if (Schema == null)
                throw new InvalidOperationException("Схема импорта не загружена.");

            return _importPipeline.BuildJson(Schema);
        }

        public RepositoryExplorerControlVM BuildRepositoryPreview(string? targetExistingRootName)
        {
            if (Schema == null)
                throw new InvalidOperationException("Схема импорта не загружена.");

            return _importPipeline.BuildRepositoryPreview(Schema, targetExistingRootName);
        }

        public string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            return _importPipeline.BuildPreviewSummary(previewViewModel);
        }

        public void ImportToRepository(
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            TreeRootModel? existingRoot)
        {
            if (Schema == null)
                throw new InvalidOperationException("Схема импорта не загружена.");

            _importPipeline.ImportToRepository(Schema, repository, repositoryService, existingRoot);
        }
    }
}
