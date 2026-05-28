using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public class ExcelImportPipeline
    {
        private readonly ConversionService _conversionService;
        private readonly IExcelImportProfileValidator _profileValidator;
        private readonly ExcelImportRepositoryPreviewBuilder _repositoryPreviewBuilder;

        public ExcelImportPipeline(
            ConversionService conversionService,
            IExcelImportProfileValidator profileValidator,
            ExcelImportRepositoryPreviewBuilder repositoryPreviewBuilder)
        {
            _conversionService = conversionService;
            _profileValidator = profileValidator;
            _repositoryPreviewBuilder = repositoryPreviewBuilder;
        }

        public List<ExcelImportProfile> GetProfilesForExecution(ExcelImportSchema schema)
        {
            return ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
        }

        public ExcelImportValidationResult Validate(ExcelImportSchema schema)
        {
            ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema);
            var profiles = GetProfilesForExecution(schema);
            return _profileValidator.ValidateProfiles(schema.SourceFilePath, profiles);
        }

        public string BuildJson(ExcelImportSchema schema)
        {
            EnsureValid(schema);
            return _conversionService.ProcessSchema(ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema));
        }

        public RepositoryExplorerControlVM BuildRepositoryPreview(
            ExcelImportSchema schema,
            string? targetExistingRootName)
        {
            var json = BuildJson(schema);
            return _repositoryPreviewBuilder.Build(json, targetExistingRootName);
        }

        public string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            return _repositoryPreviewBuilder.BuildSummary(previewViewModel);
        }

        public void ImportToRepository(
            ExcelImportSchema schema,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            TreeRootModel? existingRoot)
        {
            var json = BuildJson(schema);
            JsonImportExportHelper.ParseJson(json, repositoryService, repository, _ => { }, (_, _) => { });
        }

        private void EnsureValid(ExcelImportSchema schema)
        {
            var validationResult = Validate(schema);
            if (validationResult.HasErrors)
                throw new InvalidOperationException(ExcelImportValidationMessageBuilder.Build(validationResult));
        }

        public static void EnsureHasEnabledSheets(ExcelImportSchema schema, string emptySelectionMessage)
        {
            var profiles = ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
            if (profiles.Count == 0)
                throw new InvalidOperationException(emptySelectionMessage);
        }
    }
}
